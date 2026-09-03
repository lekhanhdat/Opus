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
using AngleSharp.Text;
using AvePoint.GCommon.Contract.PlatformRecovery.Object;
using AvePoint.GCommon.Contract.Server.Job.Object;
using AvePoint.GCommon.Contract.Tree.Object.Compare;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Discovery.DiscoveryExtension;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.SharePoint.Object;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using AvePoint.RA.DB.Dao.Extension;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Management.Automation.Language;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ZXing;
using AvePoint.RA.RAPhysical.Import;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class JobDetailDao : IJobDetailDao
    {
        private RALogger logger = RALogger.GetInstance(typeof(JobDetailDao));
        private IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();
        public bool SaveDataIntoTable(string reportFilePath, IEnumerable<JMJobDetails> jobDetails, string insertDataSql)
        {
            using var _ = new PerformanceScope("SaveJobDetails");
            List<List<SQLiteParameter>> parameterList = jobDetails.Select(BuildSQLiteParameters).ToList();
            return SQLCommond.BatchExecuteNonQueryStable(reportFilePath, insertDataSql, parameterList);
        }

        public bool DeleteData(string reportFilePath, string delDataSql)
        {
            return SQLCommond.ExecuteNonQuery(reportFilePath, delDataSql) == 0 ? true : false;
        }

        public IEnumerable<JMJobDetails> GetData(string reportFilePath, string sqlStr, BaseJobDto jobInfo, ref long lastRowId)
        {
            using var perfomanceLogger = new PerformanceScope("GetJobDetails");
            IEnumerable<JMJobDetails> result = null;
            if (SecurityUtils.ValidateSQLiteConnectionWithBuilder(reportFilePath, out var builder))
            {
                using SQLiteConnection conn = new(builder.ConnectionString);
                conn.Open();
                try
                {
                    using SQLiteCommand cmd = new(sqlStr, conn);
                    if (jobInfo.AddValues != null)
                    {
                        foreach (var item in jobInfo.AddValues)
                        {
                            cmd.Parameters.AddWithValue(item.Key, item.Value);
                        }
                    }
                    using SQLiteDataReader sqlReader = cmd.ExecuteReader();
                    if (sqlReader.HasRows)
                    {
                        (result, lastRowId) = ConvertDomainToDetailInfo(sqlReader, jobInfo);
                    }
                }
                catch (Exception e)
                {
                    logger.Error(string.Format("{0},{1}", e.Message, e));
                }
                finally
                {
                    conn.Close();
                }
            }
            return result;
        }

        public IEnumerable<JMJobDetails> GetData(string reportFilePath, string slectDataSql, BaseJobDto jobInfo)
        {
            using var perfomanceLogger = new PerformanceScope("GetJobDetails");
            IEnumerable<JMJobDetails> result = null;
            try
            {
                if (SecurityUtils.ValidateSQLiteConnectionWithBuilder(reportFilePath, out var builder))
                {
                    using (SQLiteConnection conn = new SQLiteConnection(builder.ConnectionString))
                    {
                        conn.Open();
                        try
                        {
                            //SQLiteCommand cmd = conn.CreateCommand();
                            using (SQLiteCommand cmd = new SQLiteCommand(slectDataSql, conn))
                            {
                                if (jobInfo.AddValues != null)
                                {
                                    foreach (var item in jobInfo.AddValues)
                                    {
                                        cmd.Parameters.AddWithValue(item.Key, item.Value);
                                    }
                                }

                                using (SQLiteDataReader sqlReader = cmd.ExecuteReader())
                                {
                                    result = ConvertDomainToDetailInfo(sqlReader, jobInfo).JobDetailsList;
                                }

                            }
                            //cmd.CommandText = slectDataSql;

                        }
                        catch (Exception e)
                        {
                            logger.Error(string.Format("{0},{1}", e.Message, e));
                        }
                        finally
                        {
                            conn.Close();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(string.Format("Get detail failed. {0}", e));
            }
            return result;
        }

        public IEnumerable<JMRestoreScDetails> GetDataForSCRestoreDetail(string reportFilePath, string slectDataSql)
        {
            IEnumerable<JMRestoreScDetails> result = null;
            try
            {
                if (SecurityUtils.ValidateSQLiteConnectionWithBuilder(reportFilePath, out var builder))
                {
                    using (SQLiteConnection conn = new SQLiteConnection(builder.ConnectionString))
                    {
                        conn.Open();
                        try
                        {
                            //SQLiteCommand cmd = conn.CreateCommand();
                            using (SQLiteCommand cmd = new SQLiteCommand(slectDataSql, conn))
                            {

                                using (SQLiteDataReader sqlReader = cmd.ExecuteReader())
                                {
                                    result = ConvertDomainToSCRestoreDetails(sqlReader);
                                }

                            }
                            //cmd.CommandText = slectDataSql;

                        }
                        catch (Exception e)
                        {
                            logger.Error(string.Format("{0},{1}", e.Message, e));
                        }
                        finally
                        {
                            conn.Close();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(string.Format("Get detail failed. {0}", e));
            }
            return result;
        }
        public IEnumerable<JMRestoreGDriveDetails> GetDataForGDRestoreDetail(string reportFilePath, string slectDataSql)
        {
            IEnumerable<JMRestoreGDriveDetails> result = null;
            try
            {
                if (SecurityUtils.ValidateSQLiteConnectionWithBuilder(reportFilePath, out var builder))
                {
                    using (SQLiteConnection conn = new SQLiteConnection(builder.ConnectionString))
                    {
                        conn.Open();
                        try
                        {
                            //SQLiteCommand cmd = conn.CreateCommand();
                            using (SQLiteCommand cmd = new SQLiteCommand(slectDataSql, conn))
                            {

                                using (SQLiteDataReader sqlReader = cmd.ExecuteReader())
                                {
                                    result = ConvertDomainToGDRestoreDetails(sqlReader);
                                }

                            }
                            //cmd.CommandText = slectDataSql;

                        }
                        catch (Exception e)
                        {
                            logger.Error(string.Format("{0},{1}", e.Message, e));
                        }
                        finally
                        {
                            conn.Close();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(string.Format("Get detail failed. {0}", e));
            }
            return result;
        }

        public IEnumerable<JMJobDetails> GetDataForTermSelection(string reportFilePath, string slectDataSql, BaseJobDto jobInfo)
        {
            List<JMTermSelection> result = new List<JMTermSelection>();
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection("Data Source=" + reportFilePath))
                {
                    conn.Open();
                    try
                    {

                        SQLiteCommand cmd = conn.CreateCommand();
                        cmd.CommandText = slectDataSql;
                        SQLiteDataReader sqlReader = cmd.ExecuteReader();
                        //cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Dispose();
                        while (sqlReader.Read())
                        {
                            result.Add(ConvertDomainToJMTermSelection(sqlReader));
                        }
                        sqlReader.Close();
                    }
                    catch (Exception e)
                    {
                        logger.Error(string.Format("{0},{1}", e.Message, e));
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(string.Format("Get detail failed. {0}", e));
            }
            return result;
        }

        public JMJobDetails GetDataForSOSummaryDetails(string reportFilePath, string slectDataSql, BaseJobDto jobInfo)
        {
            JMJobDetails result = null;
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection("Data Source=" + reportFilePath))
                {
                    conn.Open();
                    try
                    {
                        //SQLiteCommand cmd = conn.CreateCommand();
                        using (SQLiteCommand cmd = new SQLiteCommand(slectDataSql, conn))
                        {
                            if (jobInfo.AddValues != null)
                            {
                                foreach (var item in jobInfo.AddValues)
                                {
                                    cmd.Parameters.AddWithValue(item.Key, item.Value);
                                }
                            }

                            using (SQLiteDataReader sqlReader = cmd.ExecuteReader())
                            {
                                if (sqlReader.Read())
                                {
                                    result = ConvertDomainToSOSummaryDetails(sqlReader);
                                }
                            }
                        }
                        //cmd.CommandText = slectDataSql;

                    }
                    catch (Exception e)
                    {
                        logger.Error(string.Format("{0},{1}", e.Message, e));
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(string.Format("Get detail failed. {0}", e));
            }
            return result;
        }

        public JMJobDetails GetDataForRestoreSummaryDetails(string reportFilePath, string slectDataSql, BaseJobDto jobInfo)
        {
            JMJobDetails result = null;
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection("Data Source=" + reportFilePath))
                {
                    conn.Open();
                    try
                    {
                        //SQLiteCommand cmd = conn.CreateCommand();
                        using (SQLiteCommand cmd = new SQLiteCommand(slectDataSql, conn))
                        {
                            if (jobInfo.AddValues != null)
                            {
                                foreach (var item in jobInfo.AddValues)
                                {
                                    cmd.Parameters.AddWithValue(item.Key, item.Value);
                                }
                            }

                            using (SQLiteDataReader sqlReader = cmd.ExecuteReader())
                            {
                                if (sqlReader.Read())
                                {
                                    result = ConvertDomainToRestoreSummaryDetails(sqlReader);
                                }
                            }
                        }
                        //cmd.CommandText = slectDataSql;

                    }
                    catch (Exception e)
                    {
                        logger.Error(string.Format("{0},{1}", e.Message, e));
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(string.Format("Get detail failed. {0}", e));
            }
            return result;
        }

        public JMSOSummaryDetails StatisticDiscoverPrescanSummaryFromJobDatas(string reportFilePath, string slectDataSql)
        {
            JMSOSummaryDetails result = null;
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection("Data Source=" + reportFilePath))
                {
                    conn.Open();
                    try
                    {
                        using (SQLiteCommand cmd = new SQLiteCommand(slectDataSql, conn))
                        {

                            using (SQLiteDataReader sqlReader = cmd.ExecuteReader())
                            {
                                result = ConvertStatisticDatasToDiscoverPrescanSummary(sqlReader);
                            }
                        }

                    }
                    catch (Exception e)
                    {
                        logger.Error(string.Format("{0},{1}", e.Message, e));
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(string.Format("Get detail failed. {0}", e));
            }
            return result;
        }

        public JMJobDetails GetDataForArchiverDedupReportSummaryDetails(string reportFilePath, string slectDataSql, BaseJobDto jobInfo)
        {
            JMJobDetails result = null;
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection("Data Source=" + reportFilePath))
                {
                    conn.Open();
                    try
                    {
                        //SQLiteCommand cmd = conn.CreateCommand();
                        using (SQLiteCommand cmd = new SQLiteCommand(slectDataSql, conn))
                        {
                            if (jobInfo.AddValues != null)
                            {
                                foreach (var item in jobInfo.AddValues)
                                {
                                    cmd.Parameters.AddWithValue(item.Key, item.Value);
                                }
                            }

                            using (SQLiteDataReader sqlReader = cmd.ExecuteReader())
                            {
                                if (sqlReader.Read())
                                {
                                    result = ConvertDomainToArchiverDedupReportSummaryDetails(sqlReader);
                                }
                            }
                        }
                        //cmd.CommandText = slectDataSql;

                    }
                    catch (Exception e)
                    {
                        logger.Error(string.Format("{0},{1}", e.Message, e));
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(string.Format("Get detail failed. {0}", e));
            }
            return result;
        }

        public IEnumerable<JMRestoreScDetails> ConvertDomainToSCRestoreDetails(SQLiteDataReader sqlReader)
        {
            List<JMRestoreScDetails> result = new List<JMRestoreScDetails>();
            while (sqlReader.Read())
            {
                result.Add(ConvertDomainToJMSCRestoreDetails(sqlReader));
            }
            return result;
        }
        public IEnumerable<JMRestoreGDriveDetails> ConvertDomainToGDRestoreDetails(SQLiteDataReader sqlReader)
        {
            List<JMRestoreGDriveDetails> result = new List<JMRestoreGDriveDetails>();
            while (sqlReader.Read())
            {
                result.Add(ConvertDomainToJMGDRestoreDetails(sqlReader));
            }
            return result;
        }

        public (IEnumerable<JMJobDetails> JobDetailsList, long LastRowId) ConvertDomainToDetailInfo(SQLiteDataReader sqlReader, BaseJobDto jobInfo)
        {
            using var _ = new PerformanceScope("ConvertDomainToDetailInfo");
            if (jobInfo.IsMainJob || jobInfo.IsGettingProgress)
            {
                return ConvertDomainToMainJobDetails(sqlReader, jobInfo.IsGettingProgress);
            }
            List<JMJobDetails> result = new List<JMJobDetails>();
            bool isMergeRpt = jobInfo.IsMergeRpt;
            GeneralSettingModel model = null;
            var needGetGeneralSettingJobTypes = new List<JobType>() {
                JobType.RMArchiverBackup,
                JobType.RMEndUserArchiverBackup,
                JobType.SpecifySitesArchiverBackup,
                JobType.SpecifyTeamsArchiverBackup,
                JobType.RecordsDisposal,
                JobType.OneDriveRecordsDisposal,
                JobType.ArchiverRestore,
                JobType.ArchiverToSpoRestore,
                JobType.StubArchiverRestore,
                JobType.M365InPlaceArchiverRestore,
                JobType.VeoMerge,
                JobType.SOPreScan,
                JobType.ArchiverOutPlaceRestore,
                JobType.ArchiverScan,
                JobType.ArchiverBackup,
                JobType.ArchiverDeduplication,
                JobType.ArchiverDeduplicationReport,
                JobType.ExchangeArchiverScan,
                JobType.ExchangeArchiverBackup,
                JobType.MigrationArchiverRestore,
                JobType.MigrationArchiverRetention,
                JobType.MigrationArchiverFileLevelRetention,
                JobType.MigrationArchiverBackup,
                JobType.MigrationArchiverScan,
                JobType.DiscoverOptimization,
                JobType.ArchiverByHSMXml,
                JobType.CleanUpDuplicateDatas,
                JobType.DiscoveryAOSPOptimization,
                JobType.DiscoveryPreScan,
                JobType.DiscoveryPlanProOptimization,
                JobType.DiscoveryPlanProScan,
                JobType.StubOopRestore,
                JobType.AOSPRestore,
                JobType.BoxRecordsDisposal,
                JobType.GoogleRecordsDisposal,
                JobType.ApprovalProcessArchive,
                JobType.ConvertStub,
                JobType.DeclaredRecordsMigration,
                JobType.StubDisposal,
                JobType.FSArchiverRestore,
                JobType.TeamsArchiverBackup,
                JobType.TeamsRecordsDisposal,
                JobType.TeamsArchiverRestore,
                JobType.TeamsOutPlaceRestore,
                JobType.MailBoxArchiverRestore,
                JobType.DiscoveryExportO365Profile,
                JobType.TeamsNodeSettingUpgrade,
                JobType.TeamsPreScan,
                JobType.TeamsDataUpgrade,
                JobType.GoogleArchiverRestore,
            };
            var scanBackupJobTypes = new List<JobType> {
                JobType.ArchiverBackup,
                JobType.ArchiverScan,
                JobType.ExchangeArchiverScan,
                JobType.ExchangeArchiverBackup,
                JobType.MigrationArchiverBackup,
                JobType.MigrationArchiverScan
            };
            if (needGetGeneralSettingJobTypes.Contains((JobType)jobInfo.JobType))
            {
                model = GeneralSettingService.GetGeneralSettingAsync().GetAwaiter().GetResult();
            }

            long tempLastId = 0;
            while (sqlReader.Read())
            {
                var rowIdIdx = sqlReader.GetOrdinal(JobMonitorConstants.Row_ID_COLUMN);
                if (rowIdIdx >= 0)
                {
                    tempLastId = sqlReader.GetInt64(rowIdIdx);
                }

                if (jobInfo.JobType == (int)JobType.BCSTermUsageReport || jobInfo.JobType == (int)JobType.ItemsFilesDueDisposal
                    || jobInfo.JobType == (int)JobType.EXOTermUsageReport || jobInfo.JobType == (int)JobType.EXOItemsFilesDueDisposalReport
                    || jobInfo.JobType == (int)JobType.PhysicalTermUsageReport || jobInfo.JobType == (int)JobType.PhysicalItemsFilesDueDisposalReport
                    || jobInfo.JobType == (int)JobType.FSItemsFilesDueDisposal || jobInfo.JobType == (int)JobType.FSBCSTermUsageReport
                    || jobInfo.JobType == (int)JobType.OneDriveTermUsageReport || jobInfo.JobType == (int)JobType.OneDriveItemsFilesDueDisposalReport
                    || jobInfo.JobType == (int)JobType.SPOnPremItemsFilesDueDisposal || jobInfo.JobType == (int)JobType.SPOnPremBCSTermUsageReport
                    || jobInfo.JobType == (int)JobType.DisposalReport || jobInfo.JobType == (int)JobType.TermUsageReport
                    || jobInfo.JobType == (int)JobType.BoxItemsFilesDueDisposalReport || jobInfo.JobType == (int)JobType.BoxBCSTermUsageReport
                    || jobInfo.JobType == (int)JobType.GoogleItemsFilesDueDisposalReport || jobInfo.JobType == (int)JobType.GoogleBCSTermUsageReport
                    || jobInfo.JobType == (int)JobType.TeamsItemsFilesDueDisposalReport || jobInfo.JobType == (int)JobType.TeamsBCSTermUsageReport)
                {
                    result.Add(ConvertDomainToJMReportJobDetails(sqlReader, isMergeRpt));
                }
                else if (jobInfo.JobType == (int)JobType.TermSynchronization || jobInfo.JobType == (int)JobType.PhysicalTermSynchronization || jobInfo.JobType == (int)JobType.SPOnPremTermSynchronization)
                {
                    result.Add(ConvertDomainToJMTermSyncJobDetails(sqlReader, isMergeRpt));
                }
                else if (jobInfo.JobType == (int)JobType.SharePointGlobalSetting || jobInfo.JobType == (int)JobType.SharePointScheduleSetting || jobInfo.JobType == (int)JobType.ApplySharePointSettings || jobInfo.JobType == (int)JobType.SPOnPremApplySetting || jobInfo.JobType == (int)JobType.SPOnPremApplySettingSchedule || jobInfo.JobType == (int)JobType.ApplyTeamsSettings
                    || jobInfo.JobType == (int)JobType.TeamsScheduleSetting)
                {
                    result.Add(ConvertDomainToJMGlobalSetting(sqlReader, isMergeRpt));
                }
                else if (jobInfo.JobType == (int)JobType.PhysicalFolderSynchronization)
                {
                    result.Add(ConvertDomainToJMPhysicalSyncJob(sqlReader, isMergeRpt));
                }
                else if (jobInfo.JobType == (int)JobType.UpdateLocation)
                {
                    result.Add(ConvertDomainToJMPhysicalUpdateLocationJob(sqlReader, isMergeRpt));
                }
                else if (jobInfo.JobType == (int)JobType.ImportPhysicalRecords || jobInfo.JobType == (int)JobType.PhysicalBulkInsertExport || jobInfo.JobType == (int)JobType.PhysicalBulkEditExport)
                {
                    result.Add(ConvertDomainToJMImportPhysicalRecordsJob(sqlReader, isMergeRpt));
                }
                else if (jobInfo.JobType == (int)JobType.TrimRecordsDeletion)
                {
                    result.Add(ConvertDomainToJMDeletionImportRecordsJob(sqlReader, isMergeRpt));
                }
                else if (jobInfo.JobType == (int)JobType.ImportRecordsRelated)
                {
                    result.Add(ConvertDomainToJMImportRecordsRelatedJob(sqlReader, isMergeRpt));
                }
                else if (jobInfo.JobType == (int)JobType.AvailableSpaceReport)
                {
                    result.Add(ConvertDomainToJMAvailableSpaceReportJob(sqlReader, isMergeRpt));
                }
                else if (jobInfo.JobType == (int)JobType.CreateAndDestroyedFileReport
                    || jobInfo.JobType == (int)JobType.EXOCreateAndDestroyedFileReport
                    || jobInfo.JobType == (int)JobType.PhysicalCreateAndDestroyedFileReport
                    || jobInfo.JobType == (int)JobType.FSCreateAndDestroyedFileReport
                    || jobInfo.JobType == (int)JobType.OneDriveCreateAndDestroyedFileReport
                    || jobInfo.JobType == (int)JobType.SPOnPremCreateAndDestroyedFileReport
                    || jobInfo.JobType == (int)JobType.CreateAndDestroyedReport
                    || jobInfo.JobType == (int)JobType.BoxCreateAndDestroyedFileReport
                    || jobInfo.JobType == (int)JobType.GoogleCreateAndDestroyedFileReport
                    || jobInfo.JobType == (int)JobType.TeamsCreateAndDestroyedFileReport)
                {
                    result.Add(ConvertDomainToTimeFrameReportJob(sqlReader, isMergeRpt));
                }
                else if (jobInfo.JobType == (int)JobType.ImportTermStructure || jobInfo.JobType == (int)JobType.ExportTermStructure || jobInfo.JobType == (int)JobType.ImportGoogleTermStructure)
                {
                    result.Add(ConvertDomainToJMTermImportJobDetails(sqlReader, isMergeRpt));
                }
                else if (jobInfo.JobType == (int)JobType.DiscoveryExportO365Profile)
                {
                    result.Add(ConvertDomainToJMDiscoveryExportProfileJobDetails(sqlReader, isMergeRpt, model));
                }
                else if (jobInfo.JobType == (int)JobType.ManualApproval
                    || jobInfo.JobType == (int)JobType.ManualApprovalTimer
                    || jobInfo.JobType == (int)JobType.ManualApprovalOrRejectJob
                    || jobInfo.JobType == (int)JobType.ManualExportHistoryDatasJob
                    || jobInfo.JobType == (int)JobType.ManualExportRecordsForReviewDatasJob
                    || jobInfo.JobType == (int)JobType.ManualImportUnderReviewDatasJob
                    || jobInfo.JobType == (int)JobType.ManualFolderViewActions
                    || jobInfo.JobType == (int)JobType.DeleteInvalidRecords)
                {
                    result.Add(ConvertDomainToJMManualApprovalJobDetails(sqlReader, isMergeRpt));
                }
                else if (jobInfo.JobType == (int)JobType.ArchiverFullTextIndex)
                {
                    result.Add(ConvertDomainToJMArchiverFullTextIndexJobDetails(sqlReader, isMergeRpt));
                }
                else if (jobInfo.JobType == (int)JobType.DeleteRestoredData)
                {
                    result.Add(ConvertDomainToJMArchiverDeleteRestoredDataJobDetails(sqlReader, isMergeRpt));
                }
                else if (jobInfo.JobType == (int)JobType.DiscoveryJobV2 || jobInfo.JobType == (int)JobType.DiscoveryJobV3 || jobInfo.JobType == (int)JobType.DiscoveryJobV4 || jobInfo.JobType == (int)JobType.DiscoveryJobV5)
                {
                    result.Add(ConvertDomainToJMDiscoveryJobV2Details(sqlReader, isMergeRpt));
                }
                else if (jobInfo.JobType == (int)JobType.DiscoveryGoogleJobV1)
                {
                    result.Add(ConvertDomainToJMDiscoveryGoogleJobDetails(sqlReader, isMergeRpt));
                } 
                else if (jobInfo.JobType == (int)JobType.DiscoveryAnalysisFileSystemV1)
                {
                    result.Add(ConvertDomainToJMDiscoveryFileSystemJobDetails(sqlReader, isMergeRpt));
                }
                else if (jobInfo.JobType == (int)JobType.DiscoveryProfileJob)
                {
                    result.Add(ConvertDomainToJMDiscoveryProfileJobDetails(sqlReader, isMergeRpt));
                }
                else if (jobInfo.JobType == (int)JobType.DiscoveryGoogleProfileJob)
                {
                    result.Add(ConvertDomainToJMDiscoveryGoogleProfileJobDetails(sqlReader, isMergeRpt));
                }
                else if (jobInfo.JobType == (int)JobType.SharePointOnlineDeletionSyncUpgrade ||
                        jobInfo.JobType == (int)JobType.CosmosDBDirtyDataDeleteUpgrade ||
                        jobInfo.JobType == (int)JobType.ManualFileSystemUpgrade ||
                        jobInfo.JobType == (int)JobType.SendEmailJob ||
                        jobInfo.JobType == (int)JobType.DiscoveryJob ||
                        jobInfo.JobType == (int)JobType.DiscoveryOptimizationCalculate ||
                        jobInfo.JobType == (int)JobType.DiscoveryAOSPOptimizationCalculate ||
                        jobInfo.JobType == (int)JobType.DiscoveryReCalculate)
                {

                }
                else if (jobInfo.JobType == (int)JobType.UniqueIDSettingFullSchedule
                    || jobInfo.JobType == (int)JobType.UniqueIDSettingIncrementalSchedule
                    || jobInfo.JobType == (int)JobType.TeamsUniqueIDSettingFullSchedule
                    || jobInfo.JobType == (int)JobType.TeamsUniqueIDSettingIncrementalSchedule
                    || jobInfo.JobType == (int)JobType.SPOnPremUniqueIDSettingFullSchedule
                    || jobInfo.JobType == (int)JobType.SPOnPremUniqueIDSettingIncrementalSchedule)
                {
                    result.Add(ConvertDomainToJMUniqueIDSetting(sqlReader, isMergeRpt));
                }
                else if(jobInfo.JobType == (int)JobType.ArchiverDeduplicationReport)
                {
                    result.Add(ConvertDomainToJMArchiverDedupReportDetails(sqlReader, isMergeRpt, model));
                }
                else if (jobInfo.JobType == (int)JobType.CollectionDataFull || jobInfo.JobType == (int)JobType.CollectionDataIncremental)
                {
                    result.Add(ConvertDomainToCollectionDataSetting(sqlReader, isMergeRpt));
                }
                else if (jobInfo.JobType == (int)JobType.DataSynchronisation
                    || jobInfo.JobType == (int)JobType.SPDataSynchronisationSchedule
                    || jobInfo.JobType == (int)JobType.SPOnPremDataSync
                    || jobInfo.JobType == (int)JobType.SPOnPremDataSyncSchedule
                    || jobInfo.JobType == (int)JobType.OneDriveDataSynchronisation
                    || jobInfo.JobType == (int)JobType.OneDriveDataSynchronisationSchedule
                    || jobInfo.JobType == (int)JobType.TeamsDataSynchronisation
                    || jobInfo.JobType == (int)JobType.TeamsDataSynchronisationSchedule)
                {
                    result.Add(ConvertDomainToCollectionDataSetting(sqlReader, isMergeRpt));
                }
                else if (jobInfo.JobType == (int)JobType.AzureFileShareDataSynchronisation ||
                    jobInfo.JobType == (int)JobType.AzureFileShareDataSynchronisationSchedule)
                {
                    result.Add(ConvertDomainToAzureFileShareDataSync(sqlReader, isMergeRpt));
                }
                else if (jobInfo.JobType == (int)JobType.BoxDataSynchronisation ||
                    jobInfo.JobType == (int)JobType.BoxDataSynchronisationSchedule)
                {
                    result.Add(ConvertDomainToBoxDataSync(sqlReader, isMergeRpt));
                }
                else if(jobInfo.JobType == (int)JobType.BoxRecordsDisposal)
                {
                    result.Add(ConvertDomainToBoxDisposalActionDetail(sqlReader, isMergeRpt, model));
                }
                else if (jobInfo.JobType == (int)JobType.SyncSecurityContainer)
                {
                    result.Add(ConvertDomainToSynSecurityContainerDataSetting(sqlReader, isMergeRpt));
                }
                else if (jobInfo.JobType == (int)JobType.EnforceRetention || jobInfo.JobType == (int)JobType.OldEnforceRetention)
                {
                    result.Add(ConvertDomainToEnforceRetentionSetting(sqlReader, isMergeRpt));
                }
                else if (jobInfo.JobType == (int)JobType.RecordsExplorerMove)
                {
                    result.Add(ConvertDomainToExplorerMoveDetails(sqlReader, isMergeRpt));
                }
                else if (jobInfo.JobType == (int)JobType.EXOApplySetting || jobInfo.JobType == (int)JobType.EXOApplySettingSchedule)
                {
                    result.Add(ConvertDomainToEXOApplySettingDetails(sqlReader, isMergeRpt));
                }
                else if (jobInfo.JobType == (int)JobType.EXODataSynchronisation || jobInfo.JobType == (int)JobType.EXODataSynchronisationSchedule)
                {
                    result.Add(ConvertDomainToEXODataSyncDetails(sqlReader, isMergeRpt));
                }
                else if (jobInfo.JobType == (int)JobType.EXORecordsDisposal)
                {
                    result.Add(ConvertDomainToEXOEnforceRuleActionDetails(sqlReader, isMergeRpt));
                }
                else if (jobInfo.JobType == (int)JobType.SyncNodesFromAOS)
                {
                    result.Add(ConvertDomainToSyncRemoteNodesDetails(sqlReader, isMergeRpt));
                }
                else if (jobInfo.JobType == (int)JobType.SPOnPremScanLocalNodes)
                {
                    result.Add(ConvertDomainToScanLocalNodesDetails(sqlReader, isMergeRpt));
                }
                else if (jobInfo.JobType == (int)JobType.PhysicalRecordsDisposal)
                {
                    result.Add(ConvertDomainToPhysicalDisposalDetails(sqlReader, isMergeRpt));
                }
                else if (jobInfo.JobType == (int)JobType.PhysicalExplorerTimer)
                {
                    result.Add(ConvertDomainToPhysicalExplorerTimerDetails(sqlReader, isMergeRpt));
                }
                else if (jobInfo.JobType == (int)JobType.ConnectorTimer)
                {
                    result.Add(ConvertDomainToConnectorExplorerTimerDetails(sqlReader, isMergeRpt));
                }
                else if (jobInfo.JobType == (int)JobType.PhysicalExportBarcode)
                {
                    result.Add(ConvertDomainToExportBarcodeToLocationDetails(sqlReader, isMergeRpt));
                }
                else if (jobInfo.JobType == (int)JobType.PhysicalSetPermission)
                {
                    result.Add(ConvertDomainToPhysicalSetPermissionDetails(sqlReader, isMergeRpt));
                }
                else if (jobInfo.JobType == (int)JobType.ImportSPSetting || jobInfo.JobType == (int)JobType.ArchiverExport || jobInfo.JobType == (int)JobType.ExportSPSetting || jobInfo.JobType == (int)JobType.ExportTeamsSetting || jobInfo.JobType == (int)JobType.ImportTeamsSetting
                        || jobInfo.JobType == (int)JobType.ExportSPSOSetting || jobInfo.JobType == (int)JobType.ExportTeamsSOSetting)
                {
                    result.Add(ConvertDomainToImportSPSettingJobDetails(sqlReader));
                }
                else if (jobInfo.JobType == (int)JobType.ActionOnly)
                {
                    result.Add(ConvertDomainToActionOnlyJobDetails(sqlReader));
                }
                else if (jobInfo.JobType == (int)JobType.FSDashBoard || jobInfo.JobType == (int)JobType.FSMyHubDashboard)
                {
                    result.Add(ConvertDomainToFSDashBoardDetails(sqlReader, isMergeRpt));
                }
                else if (jobInfo.JobType == (int)JobType.SPOnPremDashBoard)
                {
                    result.Add(ConvertDomainToSPOnPremDashBoardDetails(sqlReader, isMergeRpt));
                }
                else if (jobInfo.JobType == (int)JobType.Dashboard)
                {
                    result.Add(ConvertDomainToDashboardDetails(sqlReader, isMergeRpt));
                }
                else if (jobInfo.JobType == (int)JobType.TenantUpgrade)
                {
                    result.Add(ConvertDomainToTenantUpgradeDetails(sqlReader, isMergeRpt));
                }
                else if (jobInfo.JobType == (int)JobType.ManualApprovalEmailSchedule)
                {
                    result.Add(ConvertDomainToManualApprovalSettingScheduleDetail(sqlReader, isMergeRpt));
                }
                else if (jobInfo.JobType == (int)JobType.FSDisposal
                    || jobInfo.JobType == (int)JobType.FSDisposalSchedule || jobInfo.JobType == (int)JobType.FSDisposalByClassCode)
                {
                    result.Add(ConvertDomainToFSDisposalDetails(sqlReader, isMergeRpt));
                }
                else if (jobInfo.JobType == (int)JobType.FSArchiverRestore)
                {
                    result.Add(ConvertDomainToFSRestoreDetails(sqlReader, isMergeRpt, model));
                }
                else if (jobInfo.JobType == (int)JobType.FSDataSynchronization
                    || jobInfo.JobType == (int)JobType.FSDataSynchronizationSchedule)
                {
                    result.Add(ConvertDomainToFSDataSyncDetails(sqlReader, isMergeRpt));
                }
                else if (jobInfo.JobType == (int)JobType.ImportFSSetting || jobInfo.JobType == (int)JobType.ExportFSSetting || jobInfo.JobType == (int)JobType.DownloadRCCReport)
                {
                    result.Add(ConvertDomainToFSImportSettingDetails(sqlReader, isMergeRpt));
                }
                else if (jobInfo.JobType == (int)JobType.FSFolderChangeTerm || jobInfo.JobType == (int)JobType.ApplyClassCode)
                {
                    result.Add(ConvertDomainToFSFolderReclassifyDetails(sqlReader, isMergeRpt));
                }
                else if (jobInfo.JobType == (int)JobType.FSFolderManageHold)
                {
                    result.Add(ConvertDomainToFSFolderHoldDetails(sqlReader, isMergeRpt));
                }
                else if ((JobType)jobInfo.JobType is JobType.GlobalSearchAction
                    or JobType.MachineLearningReviewApprove
                    or JobType.MachineLearningReviewReclassify
                    or JobType.MachineLearningExportReportJob
                    or JobType.ExportSiteMetrics)
                {
                    result.Add(ConvertDomainToGlobalSearchActionDetails(sqlReader, isMergeRpt));
                }
                else if (jobInfo.JobType == (int)JobType.ExportSearchResult)
                {
                    result.Add(ConvertDomainToExportSearchResultDetails(sqlReader, isMergeRpt));
                }
                else if (jobInfo.JobType == (int)JobType.SPOnPremEnforceRuleAction
                    || jobInfo.JobType == (int)JobType.SPOnPremEnforceRuleActionSchedule)
                {
                    result.Add(ConvertDomainToOnPremEnforceRuleActionDetails(sqlReader, isMergeRpt));
                }
                else if (jobInfo.JobType == (int)JobType.PhysicalLoanBox || jobInfo.JobType == (int)JobType.PhysicalReturnBox)
                {
                    result.Add(ConvertDomainToPhyLoanBoxDetails(sqlReader, isMergeRpt));
                }
                else if (jobInfo.JobType == (int)JobType.PhysicalMoveDataJob)
                {
                    result.Add(ConvertDomainToPhyMoveDetails(sqlReader, isMergeRpt));
                }
                else if (jobInfo.JobType == (int)JobType.SPOActionAuditReport || jobInfo.JobType == (int)JobType.OneDriveActionAuditReport
                    || jobInfo.JobType == (int)JobType.TeamsActionAuditReport)
                {
                    result.Add(ConvertDomainToAuditReportDetails(sqlReader, isMergeRpt));
                }
                else if (jobInfo.JobType == (int)JobType.PhysicalLoanPick || jobInfo.JobType == (int)JobType.PhysicalDestructionPick)
                {
                    result.Add(ConvertDomainToJMPickCompleteJobDetails(sqlReader, isMergeRpt));
                }
                else if (jobInfo.JobType == (int)JobType.MachineLearningTraining || jobInfo.JobType == (int)JobType.MachineLearningAnalyse)
                {
                    result.Add(ConvertDomainToJMTrainingJobDetails(sqlReader, isMergeRpt));
                }
                else if (jobInfo.JobType == (int)JobType.RMArchiverBackup
                    || jobInfo.JobType == (int)JobType.RMEndUserArchiverBackup
                    || jobInfo.JobType == (int)JobType.SpecifySitesArchiverBackup
                    || jobInfo.JobType == (int)JobType.SpecifyTeamsArchiverBackup
                    || jobInfo.JobType == (int)JobType.RecordsDisposal
                    || jobInfo.JobType == (int)JobType.OneDriveRecordsDisposal
                    || jobInfo.JobType == (int)JobType.SOPreScan
                    || jobInfo.JobType == (int)JobType.DiscoverOptimization
                    || jobInfo.JobType == (int)JobType.ArchiverByHSMXml
                    || jobInfo.JobType == (int)JobType.DiscoveryAOSPOptimization
                    || jobInfo.JobType == (int)JobType.DiscoveryPreScan
                    || jobInfo.JobType == (int)JobType.DiscoveryPlanProOptimization
                    || jobInfo.JobType == (int)JobType.DiscoveryPlanProScan
                    || jobInfo.JobType == (int)JobType.ApprovalProcessArchive
                    || jobInfo.JobType == (int)JobType.TeamsArchiverBackup
                    || jobInfo.JobType == (int)JobType.TeamsRecordsDisposal
                    || jobInfo.JobType == (int)JobType.TeamsPreScan
                    || jobInfo.JobType == (int)JobType.CleanUpDuplicateDatas
                    )
                {
                    result.Add(ConvertDomainToArchiverActionReportDetails(sqlReader, isMergeRpt, model, jobInfo.JobType));
                }
                else if (jobInfo.JobType == (int)JobType.ArchiverDeduplication)
                {
                    result.Add(ConvertDomainToArchiverDedupReportDetails(sqlReader, isMergeRpt, model));
                }
                else if (jobInfo.JobType == (int)JobType.GoogleArchiverRestore)
                {
                    result.Add(ConvertDomainToGDriveRestoreActionReportDetails(sqlReader, isMergeRpt, model, jobInfo.JobType));
                }
                else if (jobInfo.JobType == (int)JobType.ArchiverRestore 
                    || jobInfo.JobType == (int)JobType.ArchiverOutPlaceRestore 
                    || jobInfo.JobType == (int)JobType.StubOopRestore 
                    || jobInfo.JobType == (int)JobType.AOSPRestore 
                    || jobInfo.JobType == (int)JobType.TeamsArchiverRestore 
                    || jobInfo.JobType == (int)JobType.TeamsOutPlaceRestore
                    || jobInfo.JobType == (int)JobType.MailBoxArchiverRestore 
                    || jobInfo.JobType == (int)JobType.GoogleArchiverRestore
                    || jobInfo.JobType == (int)JobType.ArchiverToSpoRestore
                    || jobInfo.JobType == (int)JobType.StubArchiverRestore
                    || jobInfo.JobType == (int)JobType.M365InPlaceArchiverRestore
                    )
                {
                    result.Add(ConvertDomainToRestoreActionReportDetails(sqlReader, isMergeRpt, model, jobInfo.JobType));
                }
                else if (jobInfo.JobType == (int)JobType.ArchiverMoveIndex)
                {
                    result.Add(ConvertDomainToMergeIndexReportDetails(sqlReader, isMergeRpt, model));
                }
                else if (jobInfo.JobType == (int)JobType.VeoMerge)
                {
                    result.Add(ConvertDomainToVEOMergeDetails(sqlReader, isMergeRpt, model));
                }
                else if (jobInfo.JobType == (int)JobType.ArchiverRetention
                    || jobInfo.JobType == (int)JobType.FSRetain
                    || jobInfo.JobType == (int)JobType.TeamsArchiverRetention
                    || jobInfo.JobType == (int)JobType.EXOArchiverRetention
                    || jobInfo.JobType == (int)JobType.GoogleArchiverRetention)
                {
                    result.Add(ConvertDomainToRetentionReportDetails(sqlReader, isMergeRpt, model));
                }
                else if (jobInfo.JobType == (int)JobType.ArchiverRetentionSimulate
                    || jobInfo.JobType == (int)JobType.FSRetainSimulate)
                {
                    result.Add(ConvertDomainToRetentionDashboardReportDetails(sqlReader, isMergeRpt, model));
                }
                else if (jobInfo.JobType == (int)JobType.DeleteOrphanDatas)
                {
                    result.Add(ConvertDomainToOrphanDatasReportDetails(sqlReader, isMergeRpt, model));
                }
                else if (jobInfo.JobType == (int)JobType.RebuildStub)
                {
                    result.Add(ConvertDomainToRebuildStubReportDetails(sqlReader, isMergeRpt));
                }
                else if (jobInfo.JobType == (int)JobType.RebuildIndex)
                {
                    result.Add(ConvertDomainToRebuildIndexReportDetails(sqlReader, isMergeRpt));
                }
                else if (jobInfo.JobType == (int)JobType.CloudArchiverMigration)
                {
                    result.Add(ConvertDomainToArchiverMigrationJobDetails(sqlReader, isMergeRpt, model));
                }
                else if (scanBackupJobTypes.Contains((JobType)jobInfo.JobType))
                {
                    result.Add(ConvertDomainToJMDisposalJob(sqlReader, model, (JobType)jobInfo.JobType));
                }
                // to-do: add schedule later
                else if (jobInfo.JobType == (int)JobType.GoogleApplySettings)
                {
                    result.Add(ConvertDomainToGoogleApplySettingJob(sqlReader, isMergeRpt));
                }
                if (jobInfo.JobType == (int)JobType.MigrationArchiverRestore)
                {
                    result.Add(ConvertDomainToJMDisposalJob(sqlReader, model, (JobType)jobInfo.JobType));
                }
                if (jobInfo.JobType == (int)JobType.MigrationArchiverRetention)
                {
                    result.Add(ConvertDomainToMigrationRetentionReportDetails(sqlReader, model));
                }
                if(jobInfo.JobType == (int)JobType.MigrationArchiverFileLevelRetention)
                {
                    result.Add(ConvertDomainToMigrationFileLevelRetentionReportDetails(sqlReader, model));
                }
                if (jobInfo.JobType == (int)JobType.PhysicalDisposal)
                {
                    result.Add(ConvertDomainToMigrationPhysicalDisposalDetails(sqlReader));
                }
                if(jobInfo.JobType == (int)JobType.DownloadJobReports)
                {
                    result.Add(ConvertDomainToDownLoadDetails(sqlReader));
                }
                if (jobInfo.JobType == (int)JobType.RestoreReport || jobInfo.JobType == (int)JobType.OneDriverRestoreReport || jobInfo.JobType == (int)JobType.TeamsRestoreReport || jobInfo.JobType == (int)JobType.GoogleRestoreReport)
                {
                    result.Add(ConvertDomainToRestoreReportDetails(sqlReader));
                }
                else if (jobInfo.JobType == (int)JobType.GoogleDataSynchronization)
                {
                    result.Add(ConvertDomainToGoogleDataSyncDetails(sqlReader, isMergeRpt));
                }
                else if (jobInfo.JobType == (int)JobType.GoogleRecordsDisposal)
                {
                    result.Add(ConvertDomainToGoogleDisposalActionDetail(sqlReader, isMergeRpt, model));
                }
                else if (jobInfo.JobType == (int)JobType.SFDiscoveryJob)
                {
                    result.Add(ConvertDomainToSfDiscoveryJobDetail(sqlReader, isMergeRpt, model));
                }
                else if (jobInfo.JobType == (int)JobType.PhysicalTemplateImport)
                {
                    result.Add(ConvertDomainToJMPhysicalTemplateImportJobDetails(sqlReader, isMergeRpt));
                }
                else if (jobInfo.JobType == (int)JobType.ImportHoldRecords)
                {
                    result.Add(ConvertDomainToJMHoldRecordsImportJobDetails(sqlReader, isMergeRpt));
                }
                else if (jobInfo.JobType == (int)JobType.ImportWorkspaceHold)
                {
                    result.Add(ConvertDomainToJMWorkspaceHoldImportJobDetails(sqlReader, isMergeRpt));
                }

                else if (jobInfo.JobType == (int)JobType.ConvertStub)
                {
                    result.Add(ConvertDomainToConvertStubJobDetails(sqlReader, model));
                }
                else if (jobInfo.JobType == (int)JobType.TeamsNodeSettingUpgrade || jobInfo.JobType == (int)JobType.TeamsDataUpgrade)
                {
                    result.Add(ConvertDomainToConvertTeamsSettingUpgradeDetails(sqlReader, model));
                }
                else if (jobInfo.JobType == (int)JobType.ImportSCWhitelist || jobInfo.JobType == (int)JobType.ImportSCBlacklist
                    || jobInfo.JobType == (int)JobType.DiscoveryImportExcludeSCList)
                {
                    result.Add(ConvertDomainToImportSCWhitelistJobDetails(sqlReader, model));
                }
                else if (jobInfo.JobType == (int)JobType.DeclaredRecordsMigration)
                {
                    result.Add(ConvertDomainToDeclaredRecordsMigrationJobDetails(sqlReader, model));
                }
                else if (jobInfo.JobType == (int)JobType.StubDisposal)
                {
                    result.Add(ConvertDomainToStubDisposalJobDetails(sqlReader, model));
                }
                else if (jobInfo.JobType == (int)JobType.DeleteArchivedSiteCollection)
                {
                    result.Add(ConvertDomainToDeleteArchivedSCJobDetails(sqlReader, isMergeRpt));
                }
                else
                {
                    switch (jobInfo.JobType)
                    {
                        case (int)JobType.MultiGeoMainDCSyncCommonData:
                            result.Add(ConvertDomainToMultiGeoMainDCSyncCommonDataJobDetails(sqlReader, isMergeRpt));
                            break;
                        case (int)JobType.MultiGeoOtherDCSyncCommonData:
                            result.Add(ConvertDomainToMultiGeoOtherDCSyncCommonDataJobDetails(sqlReader, isMergeRpt));
                            break;
                        default:
                            break;
                    }
                }
            }
            return (result, tempLastId);
        }

        public (IEnumerable<JMJobDetails> JobDetailsList, long LastRowId) ConvertDomainToMainJobDetails(SQLiteDataReader sqlReader, bool isGettingProgress)
        {
            List<JMJobDetails> result = new();
            long tempLastId = 0;
            var gls = GeneralSettingService.GetGeneralSettingAsync().ExecuteAsyncTask();
            while (sqlReader.Read())
            {
                var rowIdIdx = sqlReader.GetOrdinal(JobMonitorConstants.Row_ID_COLUMN);
                if (rowIdIdx >= 0)
                {
                    tempLastId = sqlReader.GetInt64(rowIdIdx);
                }

                JMMainJobDetails detail = null;

                if (isGettingProgress)
                {
                    var tempDetails = new JMArchiverJobProgressDetails();
                    tempDetails.SubJobID = sqlReader.GetString("SubJobID");
                    tempDetails.Scope = sqlReader.GetString("Scope");
                    tempDetails.ProgressStatus = (ProgressStatus)sqlReader.GetInt64("ProgressStatus");
                    tempDetails.StartTimeStr = sqlReader.GetInt64("StartTime") == 0 ? string.Empty : GeneralSettingService.ConvertTiksToDateTime(gls, sqlReader.GetInt64("StartTime"), true).SimplifyFormatTime;
                    tempDetails.FinishTimeStr = sqlReader.GetInt64("FinishTime") == 0 ? string.Empty : GeneralSettingService.ConvertTiksToDateTime(gls, sqlReader.GetInt64("FinishTime"), true).SimplifyFormatTime;

                    tempDetails.TotalFiles = sqlReader.GetInt64("TotalFiles");

                    tempDetails.TotalMatchedRuleFilesForExport = sqlReader.GetInt64("TotalMatchedRuleFilesForExport");
                    tempDetails.TotalMatchedRuleFilesForArchive = sqlReader.GetInt64("TotalMatchedRuleFilesForArchive");
                    tempDetails.TotalMatchedRuleFilesForOtherActions = sqlReader.GetInt64("TotalMatchedRuleFilesForOtherActions");

                    var processedItemsInfoList = JsonConvert.DeserializeObject<List<ProcessedItemsInfoDto>>(sqlReader.GetString("ProcessedItemsInfos"));
                    if (processedItemsInfoList is not null && processedItemsInfoList.Count > 0)
                    {
                        tempDetails.ProcessedScannedItemsInfo = processedItemsInfoList.FirstOrDefault(x => x.Action == ActionTab.Scan) ?? new ProcessedItemsInfoDto { Action = ActionTab.Scan };
                        tempDetails.ProcessedExportedItemsInfo = processedItemsInfoList.FirstOrDefault(x => x.Action == ActionTab.Export) ?? new ProcessedItemsInfoDto { Action = ActionTab.Export };
                        tempDetails.ProcessedArchivedItemsInfo = processedItemsInfoList.FirstOrDefault(x => x.Action == ActionTab.Backup) ?? new ProcessedItemsInfoDto { Action = ActionTab.Backup };
                        tempDetails.ProcessedOtherItemsInfo = processedItemsInfoList.FirstOrDefault(x => x.Action == ActionTab.Action) ?? new ProcessedItemsInfoDto { Action = ActionTab.Action };
                    }

                    tempDetails.StartScanTime = sqlReader.GetInt64("StartScanTime") == 0 ? DateTime.MinValue : new DateTime(sqlReader.GetInt64("StartScanTime"));
                    tempDetails.EstimatedScanFinishedTime = sqlReader.GetInt64("EstimatedScanFinishedTime") == 0 ? DateTime.MinValue : new DateTime(sqlReader.GetInt64("EstimatedScanFinishedTime"));
                    tempDetails.EstimatedScanFinishedTimeStr = tempDetails.EstimatedScanFinishedTime == DateTime.MinValue ? string.Empty : GeneralSettingService.ConvertTiksToDateTime(gls, tempDetails.EstimatedScanFinishedTime.Ticks, true).SimplifyFormatTime;

                    tempDetails.StartExportTime = sqlReader.GetInt64("StartExportTime") == 0 ? DateTime.MinValue : new DateTime(sqlReader.GetInt64("StartExportTime"));
                    tempDetails.EstimatedExportFinishedTime = sqlReader.GetInt64("EstimatedExportFinishedTime") == 0 ? DateTime.MinValue : new DateTime(sqlReader.GetInt64("EstimatedExportFinishedTime"));
                    tempDetails.EstimatedExportFinishedTimeStr = tempDetails.EstimatedExportFinishedTime == DateTime.MinValue ? string.Empty : GeneralSettingService.ConvertTiksToDateTime(gls, tempDetails.EstimatedExportFinishedTime.Ticks, true).SimplifyFormatTime;

                    tempDetails.StartArchivedTime = sqlReader.GetInt64("StartArchivedTime") == 0 ? DateTime.MinValue : new DateTime(sqlReader.GetInt64("StartArchivedTime"));
                    tempDetails.EstimatedArchivedFinishedTime = sqlReader.GetInt64("EstimatedArchivedFinishedTime") == 0 ? DateTime.MinValue : new DateTime(sqlReader.GetInt64("EstimatedArchivedFinishedTime"));
                    tempDetails.EstimatedArchivedFinishedTimeStr = tempDetails.EstimatedArchivedFinishedTime == DateTime.MinValue ? string.Empty : GeneralSettingService.ConvertTiksToDateTime(gls, tempDetails.EstimatedArchivedFinishedTime.Ticks, true).SimplifyFormatTime;

                    tempDetails.StartOtherTime = sqlReader.GetInt64("StartOtherTime") == 0 ? DateTime.MinValue : new DateTime(sqlReader.GetInt64("StartOtherTime"));
                    tempDetails.EstimatedOtherFinishedTime = sqlReader.GetInt64("EstimatedOtherFinishedTime") == 0 ? DateTime.MinValue : new DateTime(sqlReader.GetInt64("EstimatedOtherFinishedTime"));
                    tempDetails.EstimatedOtherFinishedTimeStr = tempDetails.EstimatedOtherFinishedTime == DateTime.MinValue ? string.Empty : GeneralSettingService.ConvertTiksToDateTime(gls, tempDetails.EstimatedOtherFinishedTime.Ticks, true).SimplifyFormatTime;

                    tempDetails.LastUpdatedTime = sqlReader.GetInt64("LastUpdatedTime") == 0 ? DateTime.MinValue : new DateTime(sqlReader.GetInt64("LastUpdatedTime"));
                    tempDetails.LastUpdatedTimeStr = tempDetails.LastUpdatedTime == DateTime.MinValue ? string.Empty : GeneralSettingService.ConvertTiksToDateTime(gls, tempDetails.LastUpdatedTime.Ticks, true).SimplifyFormatTime;
                    detail = tempDetails;
                }
                else
                {
                    detail = new JMMainJobDetails
                    {
                        SubJobID = sqlReader.GetString("SubJobID"),
                        Status = (JobStatus)sqlReader.GetInt32("Status"),
                        Scope = sqlReader.GetString("Scope"),
                        SuccessfulCount = sqlReader.GetInt64("Successful"),
                        FailedCount = sqlReader.GetInt64("Failed"),
                        SkippedCount = sqlReader.GetInt64("Skipped"),
                    };
                }
                string comment = sqlReader.GetString("Comment");
                if (!string.IsNullOrEmpty(comment))
                {
                    detail.Comment = I18NEntity.GetString(comment);
                }
                result.Add(detail);
            }
            return (result, tempLastId);
        }

        public JMActionOnlyJobDetails ConvertDomainToActionOnlyJobDetails(SQLiteDataReader sqlReader)
        {
            JMActionOnlyJobDetails detail = new JMActionOnlyJobDetails();
            detail.ObjectName = sqlReader["ObjectName"].ToString();
            detail.Url = sqlReader["Url"].ToString();
            detail.RuleName = sqlReader["RuleName"].ToString();
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = I18NEntity.GetString(comment);
            }
            return detail;
        }

        public JMImportSPSettingDetail ConvertDomainToImportSPSettingJobDetails(SQLiteDataReader sqlReader)
        {
            JMImportSPSettingDetail detail = new JMImportSPSettingDetail();
            detail.ObjectName = sqlReader["ObjectName"].ToString();
            detail.Url = sqlReader["Url"].ToString();
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = I18NEntity.GetString(comment);
            }
            return detail;
        }


        public long GetTotalSizeForDetail(string reportFilePath, string slectDataSql, BaseJobDto jobInfo)
        {
            long result = 0;
            try
            {
                if (SecurityUtils.ValidateSQLiteConnectionWithBuilder(reportFilePath, out var builder))
                {
                    using (SQLiteConnection conn = new SQLiteConnection(builder.ConnectionString))
                    {
                        conn.Open();
                        try
                        {
                            using (SQLiteCommand cmd = new SQLiteCommand(slectDataSql, conn))
                            {
                                if (jobInfo.AddValues != null)
                                {
                                    foreach (var item in jobInfo.AddValues)
                                    {
                                        cmd.Parameters.AddWithValue(item.Key, item.Value);
                                    }
                                }

                                using (SQLiteDataReader sqlReader = cmd.ExecuteReader())
                                {
                                    if (sqlReader.Read())
                                    {
                                        result = Convert.ToInt64(sqlReader[0]);
                                    }
                                }

                            }
                        }
                        catch (Exception e)
                        {
                            logger.Error(string.Format("{0},{1}", e.Message, e));
                        }
                        finally
                        {
                            conn.Close();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(string.Format("Get detail failed. {0}", e));
            }
            return result;
        }

        public int GetCountForDetail(string reportFilePath, string slectDataSql, BaseJobDto jobInfo)
        {
            int result = 0;
            try
            {
                if (SecurityUtils.ValidateSQLiteConnectionWithBuilder(reportFilePath, out var builder))
                {
                    using (SQLiteConnection conn = new SQLiteConnection(builder.ConnectionString))
                    {
                        conn.Open();
                        try
                        {
                            using (SQLiteCommand cmd = new SQLiteCommand(slectDataSql, conn))
                            {
                                if (jobInfo.AddValues != null)
                                {
                                    foreach (var item in jobInfo.AddValues)
                                    {
                                        cmd.Parameters.AddWithValue(item.Key, item.Value);
                                    }
                                }

                                using (SQLiteDataReader sqlReader = cmd.ExecuteReader())
                                {
                                    if (sqlReader.Read())
                                    {
                                        result = int.Parse(sqlReader[0].ToString());
                                    }
                                }

                            }
                        }
                        catch (Exception e)
                        {
                            logger.Error(string.Format("{0},{1}", e.Message, e));
                        }
                        finally
                        {
                            conn.Close();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(string.Format("Get detail failed. {0}", e));
            }
            return result;
        }

        public JMReportJobDetails ConvertDomainToJMReportJobDetails(SQLiteDataReader sqlReader, bool isMergeRpt)
        {
            JMReportJobDetails detail = new JMReportJobDetails();
            detail.Type = isMergeRpt ? sqlReader["Type"].ToString() : ConvertToI18NString(sqlReader["Type"].ToString());
            detail.TitleOrName = sqlReader["TitleOrName"].ToString();
            detail.Url = JobReportUtility.ReplaceRootLocationName(sqlReader["Url"].ToString());
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            string comment = isMergeRpt ? sqlReader["Comment"].ToString() : ConvertToI18NString(sqlReader["Comment"].ToString());
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = comment;
            }
            return detail;
        }

        public JMTermSyncJobDetails ConvertDomainToJMTermSyncJobDetails(SQLiteDataReader sqlReader, bool isMergeRpt)
        {
            JMTermSyncJobDetails detail = new JMTermSyncJobDetails();
            detail.Term = isMergeRpt ? sqlReader["Term"].ToString() : ConvertToI18NString(sqlReader["Term"].ToString());
            detail.Action = isMergeRpt ? sqlReader["Action"].ToString() : ConvertToI18NString(sqlReader["Action"].ToString());
            //detail.SiteCollectionURL = sqlReader["SiteCollectionURL"].ToString();
            detail.MMSApplication = isMergeRpt ? sqlReader["MMSApplication"].ToString() : ConvertToI18NString(sqlReader["MMSApplication"].ToString());
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            detail.AgentName = GetDetailColumnValue(sqlReader, "AgentName");
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = isMergeRpt ? comment : I18NEntity.GetStringWithSeparator(comment);
            }
            return detail;
        }
        public JMGoogleLabelJobDetails ConvertDomainToJMGoogleLabelSyncJobDetails(SQLiteDataReader sqlReader, bool isMergeRpt)
        {
            JMGoogleLabelJobDetails detail = new JMGoogleLabelJobDetails();
            detail.LabelId = isMergeRpt ? sqlReader["LabelId"].ToString() : ConvertToI18NString(sqlReader["LabelId"].ToString());
            detail.LabelName = isMergeRpt ? sqlReader["LabelName"].ToString() : ConvertToI18NString(sqlReader["LabelName"].ToString());
            detail.Action = isMergeRpt ? sqlReader["Action"].ToString() : ConvertToI18NString(sqlReader["Action"].ToString());
            detail.TenantId = isMergeRpt ? sqlReader["TenantId"].ToString() : ConvertToI18NString(sqlReader["TenantId"].ToString());
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = isMergeRpt ? comment : I18NEntity.GetStringWithSeparator(comment);
            }
            return detail;
        }

        private string ConvertToI18NString(string key)
        {
            return I18NEntity.GetString(key);
        }

        public JMTermSelection ConvertDomainToJMTermSelection(SQLiteDataReader sqlReader)
        {
            JMTermSelection detail = new JMTermSelection();
            detail.Term = sqlReader["Term"].ToString();
            detail.TermFullPath = sqlReader["TermFullPath"].ToString();
            return detail;
        }

        public JMSOSummaryDetails ConvertStatisticDatasToDiscoverPrescanSummary(SQLiteDataReader sqlReader)
        {
            JMSOSummaryDetails summary = new JMSOSummaryDetails{ ActionStatistics = new List<ActionStatistics>()};
            while (sqlReader.Read())
            {
                (int actionTab, string level, int size, int count, JobDetailsStatus status, string action) data = ConvertStatisticDataToSOSummary(sqlReader);
                ActionStatistics actionStatistics = summary.ActionStatistics.FirstOrDefault(action => action.ActionTab == data.actionTab);
                if(actionStatistics == null)
                {
                    actionStatistics = new ActionStatistics();
                    summary.ActionStatistics.Add(actionStatistics);
                }
                AnalyzeStatusForSummary(actionStatistics, data.level, data.status, data.count);
                if(data.status == JobDetailsStatus.Successful)
                {
                    actionStatistics.Size += data.size;
                }
            }
            return summary;
        }

        private void AnalyzeStatusForSummary(ActionStatistics actionStatistics, string cacheNodeType, JobDetailsStatus status, int count)
        {
            switch (status)
            {
                case JobDetailsStatus.Successful:
                    AnalyzeObjCount(actionStatistics.SuccessfulObj, cacheNodeType, count);
                    break;
                case JobDetailsStatus.Skipped:
                    AnalyzeObjCount(actionStatistics.SkippedObj, cacheNodeType, count);
                    break;
                case JobDetailsStatus.Failed:
                    AnalyzeObjCount(actionStatistics.FailedObj, cacheNodeType, count);
                    break;
                default:
                    break;
            }
        }

        private void AnalyzeObjCount(ObjectStatistic objSta, string cacheNodeType, int count)
        {
            if (cacheNodeType == "RM_Archiver_JobDetailExceptionLevel")
            {
                objSta.ExceptionCount += count;
            }
            else if (cacheNodeType == "RM_JS_Rule_ObjectLevel_Attachment" || cacheNodeType == "RM_JS_Rule_ObjectLevel_ItemVersion" || cacheNodeType == "RM_JS_Rule_ObjectLevel_Item")
            {
                objSta.ItemCount += count;
            }
            else if (cacheNodeType == "RM_JS_Rule_ObjectLevel_Folder")
            {
                objSta.FolderCount += count;
            }
            else if (cacheNodeType == "RM_JS_Rule_ObjectLevel_List")
            {
                objSta.ListCount += count;
            }
            else if (cacheNodeType == "RM_JS_Rule_ObjectLevel_Site")
            {
                objSta.SiteCount += count;
            }
            else if (cacheNodeType == "RM_JS_Rule_ObjectLevel_SiteCollection")
            {
                objSta.SiteCollectionCount += count;
            }
        
    }


        public (int actionTab, string level, int size, int count, JobDetailsStatus status, string action) ConvertStatisticDataToSOSummary(SQLiteDataReader sqlReader)
        {
            int actionTab = Int32.Parse(sqlReader["ActionTab"].ToString());
            string level = sqlReader["Level"].ToString();
            string action = sqlReader["Action"].ToString();
            int size = Int32.Parse(sqlReader["Size"].ToString());
            int count = Int32.Parse(sqlReader["Count"].ToString());
            JobDetailsStatus status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            return (actionTab, level, size, count, status, action);
        }

        public JMSOSummaryDetails ConvertDomainToSOSummaryDetails(SQLiteDataReader sqlReader)
        {
            return SerializerHelper.DeserializeByJsonSerializer<JMSOSummaryDetails>(sqlReader["Statistics"].ToString());
        }

        public JMRestoreSummaryDetails ConvertDomainToRestoreSummaryDetails(SQLiteDataReader sqlReader)
        {
            return SerializerHelper.DeserializeByJsonSerializer<JMRestoreSummaryDetails>(sqlReader["Statistics"].ToString());
        }

        public JMArchiverDedupReportSummaryDetails ConvertDomainToArchiverDedupReportSummaryDetails(SQLiteDataReader sqlReader)
        {
            var summary = SerializerHelper.DeserializeByJsonSerializer<JMArchiverDedupReportSummaryDetails>(sqlReader["Statistics"].ToString());
            summary.TotalDedupFilesSizeStr = ConvertToFormatSize(summary.TotalDedupFilesSize);
            return summary;
        }

        public JMGlobalSettingJobDetails ConvertDomainToJMGlobalSetting(SQLiteDataReader sqlReader, bool isMergeRpt)
        {
            JMGlobalSettingJobDetails detail = new JMGlobalSettingJobDetails();
            detail.ObjectName = sqlReader["ObjectName"].ToString();
            detail.SourceURL = sqlReader["SourceURL"].ToString();
            detail.ColumnName = isMergeRpt ? sqlReader["ColumnName"].ToString() : ConvertToI18NString(sqlReader["ColumnName"].ToString());
            detail.Action = isMergeRpt ? sqlReader["Action"].ToString() : ConvertToI18NString(sqlReader["Action"].ToString());
            detail.AgentName = GetDetailColumnValue(sqlReader, "AgentName");
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = isMergeRpt ? comment : I18NEntity.GetString(comment);
            }
            if (-1 != sqlReader.GetOrdinal("Classification"))
            {
                detail.Classification = sqlReader["Classification"] == null ? "" : (isMergeRpt ? sqlReader["Classification"].ToString() : ConvertToI18NString(sqlReader["Classification"].ToString()));
            }
            return detail;
        }
        public JMPhysicalSyncJobDetails ConvertDomainToJMPhysicalSyncJob(SQLiteDataReader sqlReader, bool isMergeRpt)
        {
            JMPhysicalSyncJobDetails detail = new JMPhysicalSyncJobDetails();
            detail.Action = sqlReader["Action"].ToString();
            detail.Comment = sqlReader["Action"].ToString();
            detail.SiteCollectionURL = isMergeRpt ? sqlReader["SiteCollectionURL"].ToString() : ConvertToI18NString(sqlReader["SiteCollectionURL"].ToString());
            detail.LocationPath = isMergeRpt ? sqlReader["LocationPath"].ToString() : ConvertToI18NString(sqlReader["LocationPath"].ToString());
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            string comment = sqlReader["Comment"].ToString();
            detail.TermName = sqlReader["TermName"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = isMergeRpt ? comment : I18NEntity.GetString(comment);
            }
            //detail.Classification = ConvertToI18NString(sqlReader["Classification"].ToString());
            return detail;
        }
        public JMUpdateLocationJobDetail ConvertDomainToJMPhysicalUpdateLocationJob(SQLiteDataReader sqlReader, bool isMergeRpt)
        {
            JMUpdateLocationJobDetail detail = new JMUpdateLocationJobDetail();
            detail.SiteCollectionURL = isMergeRpt ? sqlReader["SiteCollectionURL"].ToString() : ConvertToI18NString(sqlReader["SiteCollectionURL"].ToString());
            detail.ItemType = isMergeRpt ? sqlReader["ItemType"].ToString() : ConvertToI18NString(sqlReader["ItemType"].ToString());
            detail.SourceUrl = JobReportUtility.ReplaceRootLocationName(sqlReader["SourceUrl"].ToString());
            detail.DestinationUrl = JobReportUtility.ReplaceRootLocationName(sqlReader["DestinationUrl"].ToString());
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = isMergeRpt ? comment : I18NEntity.GetString(comment);
            }
            return detail;
        }

        public JMImportPhysicalRecordsJobDetail ConvertDomainToJMImportPhysicalRecordsJob(SQLiteDataReader sqlReader, bool isMergeRpt)
        {
            JMImportPhysicalRecordsJobDetail detail = new JMImportPhysicalRecordsJobDetail();
            detail.SrcRecordType = ConvertToI18NString(sqlReader["SrcRecordType"].ToString());
            detail.DestRecordType = ConvertToI18NString(sqlReader["DestRecordType"].ToString());
            detail.TemplateName = I18NEntity.GetString(sqlReader["TemplateName"].ToString());
            detail.UniqueId = sqlReader["UniqueId"].ToString();
            detail.Title = sqlReader["Title"].ToString();
            detail.Container = sqlReader["Container"].ToString();
            detail.SrcLocation = sqlReader["SrcLocation"].ToString();
            detail.LocationFullPath = JobReportUtility.ReplaceRootLocationName(sqlReader["LocationFullPath"].ToString());
            detail.Barcode = GetDetailColumnValue(sqlReader, "Barcode");
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = isMergeRpt ? comment : I18NEntity.GetString(comment);
            }
            return detail;
        }
        public JMImportedPhysicalRecordsDeletionDetail ConvertDomainToJMDeletionImportRecordsJob(SQLiteDataReader sqlReader, bool isMergeRpt)
        {
            JMImportedPhysicalRecordsDeletionDetail detail = new JMImportedPhysicalRecordsDeletionDetail();
            detail.ObjectName = sqlReader["ObjectName"].ToString();
            detail.UniqueId = sqlReader["UniqueId"].ToString();
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = isMergeRpt ? comment : I18NEntity.GetString(comment);
            }
            return detail;
        }
        public JMImportRecordsRelatedJobDetail ConvertDomainToJMImportRecordsRelatedJob(SQLiteDataReader sqlReader, bool isMergeRpt)
        {
            JMImportRecordsRelatedJobDetail detail = new JMImportRecordsRelatedJobDetail();
            detail.SrcId = ConvertToI18NString(sqlReader["SrcId"].ToString());
            detail.SrcName = sqlReader["SrcName"].ToString();
            detail.SrcType = ConvertToI18NString(sqlReader["SrcType"].ToString());
            detail.SrcItemId = ConvertToI18NString(sqlReader["SrcItemId"].ToString());
            detail.SrcItemUrl = JobReportUtility.ReplaceRootLocationName(sqlReader["SrcItemUrl"].ToString());
            detail.SrcSiteId = ConvertToI18NString(sqlReader["SrcSiteId"].ToString());
            detail.SrcLocation = ConvertToI18NString(sqlReader["SrcLocation"].ToString());
            detail.DestName = sqlReader["DestName"].ToString();
            detail.DestType = sqlReader["DestType"].ToString();
            detail.DestItemId = sqlReader["DestItemId"].ToString();
            detail.DestItemUrl = JobReportUtility.ReplaceRootLocationName(sqlReader["DestItemUrl"].ToString());
            detail.DestSiteId = sqlReader["DestSiteId"].ToString();
            detail.DestSiteUrl = JobReportUtility.ReplaceRootLocationName(sqlReader["DestSiteUrl"].ToString());
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = isMergeRpt ? comment : I18NEntity.GetString(comment);
            }
            return detail;
        }
        public JMAvailableSpaceReportJobDetail ConvertDomainToJMAvailableSpaceReportJob(SQLiteDataReader sqlReader, bool isMergeRpt)
        {
            JMAvailableSpaceReportJobDetail detail = new JMAvailableSpaceReportJobDetail();
            detail.Location = JobReportUtility.ReplaceRootLocationName(sqlReader["Location"].ToString());
            detail.LocationSize = Convert.ToDouble(sqlReader["LocationSize"]);
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = isMergeRpt ? comment : I18NEntity.GetString(comment);
            }
            return detail;
        }

        public JMCreateAndDestroyedFileReportJobDetail ConvertDomainToTimeFrameReportJob(SQLiteDataReader sqlReader, bool isMergeRpt)
        {
            JMCreateAndDestroyedFileReportJobDetail detail = new JMCreateAndDestroyedFileReportJobDetail();
            detail.ObjectLevel = isMergeRpt ? sqlReader["ObjectLevel"].ToString() : ConvertToI18NString(sqlReader["ObjectLevel"].ToString());
            detail.Title = sqlReader["Title"].ToString();
            detail.TermName = sqlReader["TermName"].ToString();
            if (-1 != sqlReader.GetOrdinal("Url"))
            {
                string URL = sqlReader["Url"] != null ? sqlReader["Url"].ToString() : string.Empty;
                detail.URL = JobReportUtility.ReplaceRootLocationName(URL);
            }
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = isMergeRpt ? comment : I18NEntity.GetString(comment);
            }
            return detail;
        }
        public JMImportTermDetail ConvertDomainToJMTermImportJobDetails(SQLiteDataReader sqlReader, bool isMergeRpt)
        {
            JMImportTermDetail detail = new JMImportTermDetail();
            detail.Term = isMergeRpt ? sqlReader["Term"].ToString() : ConvertToI18NString(sqlReader["Term"].ToString());
            detail.Action = isMergeRpt ? sqlReader["Action"].ToString() : ConvertToI18NString(sqlReader["Action"].ToString());
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = isMergeRpt ? comment : I18NEntity.GetStringWithSeparator(comment);
            }
            return detail;
        }

        public JMDiscoveryExportProfileJobDetails ConvertDomainToJMDiscoveryExportProfileJobDetails(SQLiteDataReader sqlReader, bool isMergeRpt, GeneralSettingModel generalSettingModel)
        {
            JMDiscoveryExportProfileJobDetails detail = new JMDiscoveryExportProfileJobDetails();
            detail.ProfileName = isMergeRpt ? sqlReader["ProfileName"].ToString() : ConvertToI18NString(sqlReader["ProfileName"].ToString());
            detail.ProfileCriteria = isMergeRpt ? sqlReader["ProfileCriteria"].ToString() : ConvertDiscoveryExportCriteriaToI18NString(sqlReader["ProfileCriteria"].ToString());
            detail.Action = isMergeRpt ? sqlReader["Action"].ToString() : ConvertToI18NString(sqlReader["Action"].ToString());
            detail.FinishTime = sqlReader["FinishTime"] != DBNull.Value ? GeneralSettingService.ConvertTiksToDateTime(generalSettingModel, Int64.Parse(sqlReader["FinishTime"].ToString()), true).SimplifyFormatTime 
                               : GeneralSettingService.ConvertTiksToDateTime(generalSettingModel, DateTime.Now.Ticks, true).SimplifyFormatTime;
            detail.Status = sqlReader["Status"] == DBNull.Value ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
               detail.Comment = isMergeRpt ? comment : I18NEntity.GetStringWithSeparator(comment);
            }
            return detail;
        }
        
        private string ConvertDiscoveryExportCriteriaToI18NString(string criteria)
        {
            if (string.IsNullOrEmpty(criteria))
            {
                return string.Empty;
            }
            Dictionary<string, string> criteriaDict = new()
            {
                {"RM_FA_Discovery_ConfigFilter_TimeRange", I18NEntity.GetString("RM_FA_Discovery_ConfigFilter_TimeRange")},
                {"RM_DA_Profile_ProfileFileSize", I18NEntity.GetString("RM_DA_Profile_ProfileFileSize")},
                {"RM_FA_ROTRule_Optimization_ROTrule", I18NEntity.GetString("RM_FA_ROTRule_Optimization_ROTrule")},
                {"RM_DA_Profile_ProfileFileType", I18NEntity.GetString("RM_DA_Profile_ProfileFileType")},
                {"RM_FA_Inactive_SummaryTab_ModifiedFrom", I18NEntity.GetString("RM_FA_Inactive_SummaryTab_ModifiedFrom")},
                {"RM_FA_Inactive_ModifiedOption_Latest", I18NEntity.GetString("RM_FA_Inactive_ModifiedOption_Latest")},
                {"RM_JS_RDM_CreateRule_Unit_Months", I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Months")},
                {"RM_FA_Inactive_SummaryTab_ModifiedTo", I18NEntity.GetString("RM_FA_Inactive_SummaryTab_ModifiedTo")},
                {"RM_FA_Inactive_ModifiedOption_Max", I18NEntity.GetString("RM_FA_Inactive_ModifiedOption_Max")},
                {"RM_FA_Inactive_OptimizationTab_FileSizeRangeAll", I18NEntity.GetString("RM_FA_Inactive_OptimizationTab_FileSizeRangeAll")},
            };
            foreach (var item in criteriaDict)
            {
                criteria = criteria.Replace(item.Key, item.Value);
            }

            return criteria;
        }

        public JMEnforceRetentionJobDetail ConvertDomainToEnforceRetentionSetting(SQLiteDataReader sqlReader, bool isMergeRpt)
        {
            JMEnforceRetentionJobDetail detail = new JMEnforceRetentionJobDetail();
            detail.ObjectName = sqlReader["ObjectName"].ToString();
            detail.SourceURL = sqlReader["FullPath"].ToString();
            string action = sqlReader["Action"].ToString();
            if (!string.IsNullOrEmpty(action))
            {
                detail.Action = isMergeRpt ? action : I18NEntity.GetString(action);
            }
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = isMergeRpt ? comment : I18NEntity.GetStringWithSeparator(comment);
            }
            return detail;
        }

        public JMManualApprovalJobDetails ConvertDomainToJMManualApprovalJobDetails(SQLiteDataReader sqlReader, bool isMergeRpt)
        {
            JMManualApprovalJobDetails detail = new JMManualApprovalJobDetails();
            detail.ObjectLevel = isMergeRpt ? sqlReader["ObjectLevel"].ToString() : ConvertToI18NString(sqlReader["ObjectLevel"].ToString());
            detail.TitleOrName = isMergeRpt ? sqlReader["TitleOrName"].ToString() : ConvertToI18NString(sqlReader["TitleOrName"].ToString());
            detail.Url = JobReportUtility.ReplaceRootLocationName(sqlReader["Url"].ToString());
            detail.ApprovalStatus = isMergeRpt ? sqlReader["ApprovalStatus"].ToString() : ConvertToI18NString(sqlReader["ApprovalStatus"].ToString());
            detail.Action = isMergeRpt ? sqlReader["Action"].ToString() : ConvertToI18NString(sqlReader["Action"].ToString());
            detail.RecordOwner = sqlReader["RecordOwner"].ToString();
            detail.RuleCriteria = I18NEntity.ReplaceI18NKey(sqlReader["RuleCriteria"].ToString(), "RM_JS_", new string[] { ",", " " });
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = isMergeRpt ? comment : I18NEntity.GetStringWithSeparator(comment);
            }
            return detail;
        }        
        
        public JMPhysicalTemplateImportJobDetail ConvertDomainToJMPhysicalTemplateImportJobDetails(SQLiteDataReader sqlReader, bool isMergeRpt)
        {
            JMPhysicalTemplateImportJobDetail detail = new JMPhysicalTemplateImportJobDetail();
            detail.TemplateSuiteName = sqlReader["TemplateSuiteName"].ToString();
            detail.TemplateSuiteStartFrom = isMergeRpt ? sqlReader["TemplateSuiteStartFrom"].ToString() : ConvertToI18NString(sqlReader["TemplateSuiteStartFrom"].ToString());
            detail.TemplateName = sqlReader["TemplateName"].ToString();
            detail.TemplateType = isMergeRpt ? sqlReader["TemplateType"].ToString() : ConvertToI18NString(sqlReader["TemplateType"].ToString());
            detail.TemplatePrefix = sqlReader["TemplatePrefix"].ToString();
            detail.TemplateDigits = sqlReader["TemplateDigits"].ToString() == "0" ? string.Empty : sqlReader["TemplateDigits"].ToString();
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = isMergeRpt ? comment : I18NEntity.GetStringWithSeparator(comment);
            }
            return detail;
        }
        public JMHoldRecordsImportJobDetail ConvertDomainToJMHoldRecordsImportJobDetails(SQLiteDataReader sqlReader, bool isMergeRpt)
        {
            JMHoldRecordsImportJobDetail detail = new JMHoldRecordsImportJobDetail();
            detail.Name = sqlReader["Name"].ToString();
            detail.Url = sqlReader["Url"].ToString();
            detail.HoldTitle = sqlReader["HoldTitle"].ToString();
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = isMergeRpt ? comment : I18NEntity.GetStringWithSeparator(comment);
            }
            return detail;
        }
        public JMArchiverDeleteRestoredDataJobDetails ConvertDomainToJMArchiverDeleteRestoredDataJobDetails(SQLiteDataReader sqlReader, bool isMergeRpt)
        {
            var res = new JMArchiverDeleteRestoredDataJobDetails
            {
                Url = sqlReader["Url"].ToString(),
                RestoredUrl = sqlReader["RestoredUrl"].ToString(),
                CleanOption = I18NEntity.GetString(sqlReader["CleanOption"].ToString()),
                CleanDelayDays = Convert.ToInt32(sqlReader["CleanDelayDays"]),
                IsRelatedDelete = I18NEntity.GetString(sqlReader["IsRelatedDelete"].ToString()),
                Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString()),
            };
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                res.Comment = isMergeRpt ? comment : I18NEntity.GetString(comment);
            }
            return res;
        }

        public JMArchiverFullTextIndexJobDetails ConvertDomainToJMArchiverFullTextIndexJobDetails(SQLiteDataReader sqlReader, bool isMergeRpt)
        {
            var res = new JMArchiverFullTextIndexJobDetails()
            {
                Url = sqlReader["Url"].ToString(),
                Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString()),
            };
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                res.Comment = isMergeRpt ? comment : I18NEntity.GetString(comment);
            }

            return res;
        }

        public JMDiscoveryJobV2Details ConvertDomainToJMDiscoveryJobV2Details(SQLiteDataReader sqlReader, bool isMergeRpt)
        {
            var res = new JMDiscoveryJobV2Details()
            {
                Url = sqlReader["Url"].ToString(),
                Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString()),
            };
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                res.Comment = isMergeRpt ? comment : I18NEntity.GetString(comment);
            }

            return res;
        }

        public JMDiscoveryGoogleJobDetails ConvertDomainToJMDiscoveryGoogleJobDetails(SQLiteDataReader sqlReader, bool isMergeRpt)
        {
            var res = new JMDiscoveryGoogleJobDetails()
            {
                DriveName = sqlReader["DriveName"].ToString(),
                Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString()),
            };
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                res.Comment = isMergeRpt ? comment : I18NEntity.GetString(comment);
            }

            return res;
        }

        public JMDiscoveryFileSystemJobDetails ConvertDomainToJMDiscoveryFileSystemJobDetails(SQLiteDataReader sqlReader, bool isMergeRpt)
        {
            var res = new JMDiscoveryFileSystemJobDetails()
            {
                ConnectionName = sqlReader["ConnectionName"].ToString(),
                Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString()),
            };
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                res.Comment = isMergeRpt ? comment : I18NEntity.GetString(comment);
            }

            return res;
        }

        public JMDiscoveryProfileJobDetails ConvertDomainToJMDiscoveryProfileJobDetails(SQLiteDataReader sqlReader, bool isMergeRpt)
        {
            var res = new JMDiscoveryProfileJobDetails()
            {
                ProfileName = sqlReader["ProfileName"].ToString(),
                Url = sqlReader["Url"].ToString(),
                Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString()),
            };
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                res.Comment = isMergeRpt ? comment : I18NEntity.GetString(comment);
            }

            return res;
        }

        public JMDiscoveryGoogleProfileJobDetails ConvertDomainToJMDiscoveryGoogleProfileJobDetails(SQLiteDataReader sqlReader, bool isMergeRpt)
        {
            var res = new JMDiscoveryGoogleProfileJobDetails()
            {
                ProfileName = sqlReader["ProfileName"].ToString(),
                DriveName = sqlReader["DriveName"].ToString(),
                Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString()),
            };
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                res.Comment = isMergeRpt ? comment : I18NEntity.GetString(comment);
            }

            return res;
        }

        public JMUniqueIDSettingJobDetails ConvertDomainToJMUniqueIDSetting(SQLiteDataReader sqlReader, bool isMergeRpt)
        {
            JMUniqueIDSettingJobDetails detail = new JMUniqueIDSettingJobDetails();
            detail.ObjectName = sqlReader["ObjectName"].ToString();
            detail.SourceURL = sqlReader["SourceURL"].ToString();
            detail.ColumnName = isMergeRpt ? sqlReader["ColumnName"].ToString() : ConvertToI18NString(sqlReader["ColumnName"].ToString());
            detail.Action = isMergeRpt ? sqlReader["Action"].ToString() : ConvertToI18NString(sqlReader["Action"].ToString());
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            detail.AgentName = GetDetailColumnValue(sqlReader, "AgentName");
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = isMergeRpt ? comment : I18NEntity.GetString(comment);
            }
            if (-1 != sqlReader.GetOrdinal("UniqueID"))
            {
                detail.UniqueID = sqlReader["UniqueID"] == null ? "" : sqlReader["UniqueID"].ToString();
            }
            return detail;
        }

        public JMArchiverDedupReportDetails ConvertDomainToJMArchiverDedupReportDetails(SQLiteDataReader sqlReader, bool isMergeRpt, GeneralSettingModel generalSettingModel)
        {
            JMArchiverDedupReportDetails detail = new JMArchiverDedupReportDetails();
            detail.Date = Int64.Parse(sqlReader["Date"].ToString());
            detail.DateStr = GeneralSettingService.ConvertTiksToDateTime(generalSettingModel, detail.Date, true).SimplifyFormatTime;
            detail.Size = Int64.Parse(sqlReader["Size"].ToString());
            detail.SizeStr = ConvertToFormatSize(detail.Size);
            detail.SrcURL = sqlReader["SrcURL"].ToString();
            detail.SubJobId = sqlReader["SubJobId"].ToString();
            detail.Remark1 = Int64.Parse(sqlReader["Remark1"].ToString());
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = isMergeRpt ? comment : I18NEntity.GetString(comment);
            }
            return detail;
        }

        public JMCollectionDataJobDetails ConvertDomainToCollectionDataSetting(SQLiteDataReader sqlReader, bool isMergeRpt)
        {
            JMCollectionDataJobDetails detail = new JMCollectionDataJobDetails();
            detail.ObjectName = sqlReader["ObjectName"].ToString();
            detail.FullPath = sqlReader["FullPath"].ToString();
            detail.AgentName = GetDetailColumnValue(sqlReader, "AgentName");
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = isMergeRpt ? comment : I18NEntity.GetStringWithSeparator(comment);
            }
            return detail;
        }

        public JMSyncSecurityContainerJobDetails ConvertDomainToSynSecurityContainerDataSetting(SQLiteDataReader sqlReader, bool isMergeRpt)
        {
            JMSyncSecurityContainerJobDetails detail = new JMSyncSecurityContainerJobDetails();
            detail.ObjectName = DefaultSecurityContainerNameHelper.GetI18NName(sqlReader["ObjectName"].ToString());
            detail.FullPath = sqlReader["FullPath"].ToString();
            detail.Container = DefaultSecurityContainerNameHelper.GetI18NName(sqlReader["Container"].ToString());
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = isMergeRpt ? comment : I18NEntity.GetString(comment);
            }
            return detail;
        }

        public JMAzureFileShareDataSyncDetail ConvertDomainToAzureFileShareDataSync(SQLiteDataReader sqlReader, bool isMergeRpt)
        {
            return new JMAzureFileShareDataSyncDetail
            {
                ObjectName = sqlReader["ObjectName"].ToString(),
                FullPath = sqlReader["FullPath"].ToString(),
                ItemType = isMergeRpt ? sqlReader["NodeType"].ToString() : I18NEntity.GetString(sqlReader["NodeType"].ToString()),
                Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString()),
                Comment = sqlReader["Comment"] == null ? "" : isMergeRpt ? sqlReader["Comment"].ToString() : I18NEntity.GetStringWithSeparator(sqlReader["Comment"].ToString())
            };
        }

        public JMBoxDataSyncDetail ConvertDomainToBoxDataSync(SQLiteDataReader sqlReader, bool isMergeRpt)
        {
            return new JMBoxDataSyncDetail
            {
                ObjectName = sqlReader["ObjectName"].ToString(),
                FullPath = sqlReader["FullPath"].ToString(),
                ItemType = isMergeRpt ? sqlReader["NodeType"].ToString() : I18NEntity.GetString(sqlReader["NodeType"].ToString()),
                Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString()),
                Comment = sqlReader["Comment"] == null ? "" : isMergeRpt ? sqlReader["Comment"].ToString() : I18NEntity.GetStringWithSeparator(sqlReader["Comment"].ToString())
            };
        }

        public JMArchiverActionJobDetails ConvertDomainToBoxDisposalActionDetail(SQLiteDataReader sqlReader, bool isMergeRpt, GeneralSettingModel generalSettingModel)
        {
            JMArchiverActionJobDetails detail = new JMArchiverActionJobDetails();
            detail.ActionTab = Int32.Parse(sqlReader["ActionTab"].ToString());
            detail.Level = isMergeRpt ? sqlReader["Level"].ToString() : I18NEntity.GetString(sqlReader["Level"].ToString());
            detail.Size = sqlReader["Size"].ToString();
            detail.SizeStr = isMergeRpt ? "" : ConvertToFormatSize(long.Parse(detail.Size));
            detail.FinishTime = sqlReader["FinishTime"] == null ? 0 : long.Parse(sqlReader["FinishTime"].ToString());
            if (generalSettingModel != null)
            {
                detail.FinishTimeStr = isMergeRpt ? "" : GeneralSettingService.ConvertTiksToDateTime(generalSettingModel, detail.FinishTime, true).SimplifyFormatTime;
            }
            else
            {
                detail.FinishTimeStr = isMergeRpt ? "" : detail.FinishTime.ToString();
            }
            detail.SourceLocation = isMergeRpt ? sqlReader["SourceLocation"].ToString() : JobReportUtility.ReplaceRootLocationName(sqlReader["SourceLocation"].ToString());
            detail.DestinationLocation = sqlReader["DestinationLocation"].ToString();
            detail.Action = isMergeRpt ? sqlReader["Action"].ToString() : I18NEntity.GetString(sqlReader["Action"].ToString());
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = isMergeRpt ? comment : I18NEntity.GetStringWithSeparator(comment);
            }
            detail.RuleName = sqlReader["RuleName"].ToString();
            return detail;
        }

        public JMGoogleJobDetails ConvertDomainToGoogleApplySettingJob(SQLiteDataReader sqlReader, bool isMergeRpt)
        {
            return new JMGoogleJobDetails
            {
                ObjectName = sqlReader["ObjectName"].ToString(),
                FullPath = sqlReader["FullPath"].ToString(),
                Classification = sqlReader["Classification"].ToString(),
                FileSize = sqlReader["FileSize"] == null ? "0" : sqlReader["FileSize"].ToString(),
                ItemType = isMergeRpt ? sqlReader["NodeType"].ToString() : I18NEntity.GetString(sqlReader["NodeType"].ToString()),
                Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString()),
                Action = sqlReader["Action"] == null ? "" : isMergeRpt ? sqlReader["Action"].ToString() : I18NEntity.GetStringWithSeparator(sqlReader["Action"].ToString()),
                Comment = sqlReader["Comment"] == null ? "" : isMergeRpt ? sqlReader["Comment"].ToString() : I18NEntity.GetStringWithSeparator(sqlReader["Comment"].ToString())
            };
        }

        public JMArchiverActionJobDetails ConvertDomainToGoogleDisposalActionDetail(SQLiteDataReader sqlReader, bool isMergeRpt, GeneralSettingModel generalSettingModel)
        {
            JMArchiverActionJobDetails detail = new JMArchiverActionJobDetails();
            detail.ActionTab = Int32.Parse(sqlReader["ActionTab"].ToString());
            detail.Level = isMergeRpt ? sqlReader["Level"].ToString() : I18NEntity.GetString(sqlReader["Level"].ToString());
            detail.Size = string.IsNullOrEmpty(sqlReader["Size"].ToString()) ? "0" : sqlReader["Size"].ToString();
            detail.SizeStr = isMergeRpt ? "" : ConvertToFormatSize(long.Parse(detail.Size));
            detail.FinishTime = sqlReader["FinishTime"] == null ? 0 : long.Parse(sqlReader["FinishTime"].ToString());
            if (generalSettingModel != null)
            {
                detail.FinishTimeStr = isMergeRpt ? "" : GeneralSettingService.ConvertTiksToDateTime(generalSettingModel, detail.FinishTime, true).SimplifyFormatTime;
            }
            else
            {
                detail.FinishTimeStr = isMergeRpt ? "" : detail.FinishTime.ToString();
            }
            detail.SourceLocation = isMergeRpt ? sqlReader["SourceLocation"].ToString() : JobReportUtility.ReplaceRootLocationName(sqlReader["SourceLocation"].ToString());
            detail.DestinationLocation = sqlReader["DestinationLocation"].ToString();
            detail.Action = isMergeRpt ? sqlReader["Action"].ToString() : I18NEntity.GetString(sqlReader["Action"].ToString());
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            string comment = isMergeRpt ? sqlReader["Comment"].ToString() : ConvertToI18NString(sqlReader["Comment"].ToString());
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = isMergeRpt ? comment : I18NEntity.GetStringWithSeparator(comment);
            }
            detail.RuleName = sqlReader["RuleName"].ToString();
            return detail;
        }
        
        public JMSalesforceDiscoveryJob ConvertDomainToSfDiscoveryJobDetail(SQLiteDataReader sqlReader, bool isMergeRpt, GeneralSettingModel generalSettingModel)
        {
            JMSalesforceDiscoveryJob detail = new JMSalesforceDiscoveryJob();
            detail.ObjectName = sqlReader["ObjectName"].ToString();
            detail.ObjectType = isMergeRpt ? sqlReader["ObjectType"].ToString() : ConvertToI18NString(sqlReader["ObjectType"].ToString());
            detail.TotalItemCount = long.Parse(sqlReader["TotalItemCount"].ToString());
            detail.TotalSize = long.Parse(sqlReader["TotalSize"].ToString());
            detail.TenantId = sqlReader["TenantId"].ToString();
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            detail.Comment = isMergeRpt ? sqlReader["Comment"].ToString() : ConvertToI18NString(sqlReader["Comment"].ToString());
            return detail;
        }

        public JMGoogleDataSyncJobDetails ConvertDomainToGoogleDataSyncDetails(SQLiteDataReader sqlReader, bool isMergeRpt)
        {
            return new JMGoogleDataSyncJobDetails
            {
                ObjectName = sqlReader["ObjectName"].ToString(),
                FullPath = sqlReader["FullPath"].ToString(),
                Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString()),
                ItemType = isMergeRpt ? GetDetailColumnValue(sqlReader, "ItemType") : ConvertToI18NString(GetDetailColumnValue(sqlReader, "ItemType")),
                Comment = sqlReader["Comment"] == null ? "" : isMergeRpt ? sqlReader["Comment"].ToString() : I18NEntity.GetStringWithSeparator(sqlReader["Comment"].ToString())
            };
        }

        public JMExplorerMoveJobDetails ConvertDomainToExplorerMoveDetails(SQLiteDataReader sqlReader, bool isMergeRpt)
        {
            JMExplorerMoveJobDetails detail = new JMExplorerMoveJobDetails();
            detail.ObjectName = sqlReader["ObjectName"].ToString();
            detail.ItemType = isMergeRpt ? sqlReader["ItemType"].ToString() : ConvertToI18NString(sqlReader["ItemType"].ToString());
            detail.FullPath = JobReportUtility.ReplaceRootLocationName(sqlReader["FullPath"].ToString());
            detail.DestinationFullPath = JobReportUtility.ReplaceRootLocationName(sqlReader["DestinationFullPath"].ToString());
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = isMergeRpt ? comment : I18NEntity.GetString(comment);
            }
            return detail;
        }

        public JMEXOApplySettingJobDetails ConvertDomainToEXOApplySettingDetails(SQLiteDataReader sqlReader, bool isMergeRpt)
        {
            JMEXOApplySettingJobDetails detail = new JMEXOApplySettingJobDetails();
            detail.ObjectName = sqlReader["ObjectName"].ToString();
            detail.ItemType = isMergeRpt ? sqlReader["ItemType"].ToString() : JobReportUtility.ConvertItemTypeStringForEXODetails(sqlReader["ItemType"].ToString());
            detail.FullPath = sqlReader["FullPath"].ToString();
            detail.Classification = GetDetailColumnValue(sqlReader,"Classification");
            string action = sqlReader["Action"].ToString();
            if (!string.IsNullOrEmpty(action))
            {
                detail.Action = isMergeRpt ? action : I18NEntity.GetString(action);
            }
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = isMergeRpt ? comment : I18NEntity.GetString(comment);
            }
            return detail;
        }

        public JMEXODataSyncJobDetails ConvertDomainToEXODataSyncDetails(SQLiteDataReader sqlReader, bool isMergeRpt)
        {
            JMEXODataSyncJobDetails detail = new JMEXODataSyncJobDetails();
            detail.ObjectName = sqlReader["ObjectName"].ToString();
            detail.ItemType = isMergeRpt ? sqlReader["ItemType"].ToString() : JobReportUtility.ConvertItemTypeStringForEXODetails(sqlReader["ItemType"].ToString());
            detail.FullPath = sqlReader["FullPath"].ToString();
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = isMergeRpt ? comment : I18NEntity.GetString(comment);
            }
            return detail;
        }

        public JMEXOEnforceRuleActionJobDetails ConvertDomainToEXOEnforceRuleActionDetails(SQLiteDataReader sqlReader, bool isMergeRpt)
        {
            JMEXOEnforceRuleActionJobDetails detail = new JMEXOEnforceRuleActionJobDetails();
            detail.Action = JobReportUtility.ConvertStringForDetails(sqlReader["Action"].ToString(), isMergeRpt);
            detail.ObjectName = sqlReader["ObjectName"].ToString();
            detail.ItemType = isMergeRpt ? sqlReader["ItemType"].ToString() : JobReportUtility.ConvertItemTypeStringForEXODetails(sqlReader["ItemType"].ToString());
            detail.FullPath = sqlReader["FullPath"].ToString();
            detail.RuleName = sqlReader["RuleName"].ToString();
            detail.DestinationUrl = sqlReader["DestinationUrl"].ToString();
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = isMergeRpt ? comment : I18NEntity.GetStringWithSeparator(comment);
            }
            return detail;
        }

        public JMSyncRemoteNodesJobDetails ConvertDomainToSyncRemoteNodesDetails(SQLiteDataReader sqlReader, bool isMergeRpt)
        {
            JMSyncRemoteNodesJobDetails detail = new JMSyncRemoteNodesJobDetails();
            detail.ObjectName = JobReportUtility.ConvertStringForDetails(sqlReader["ObjectName"].ToString(), isMergeRpt);
            detail.Container = JobReportUtility.ConvertStringForDetails(sqlReader["Container"].ToString(), isMergeRpt);
            detail.ItemType = JobReportUtility.ConvertStringForDetails(sqlReader["ItemType"].ToString(), isMergeRpt);
            detail.Action = JobReportUtility.ConvertStringForDetails(sqlReader["Action"].ToString(), isMergeRpt);
            detail.Comment = JobReportUtility.ConvertStringForDetails(sqlReader["Comment"].ToString(), isMergeRpt);
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            return detail;
        }

        public JMScanLocalNodesJobDetails ConvertDomainToScanLocalNodesDetails(SQLiteDataReader sqlReader, bool isMergeRpt)
        {
            JMScanLocalNodesJobDetails detail = new JMScanLocalNodesJobDetails();
            detail.ObjectName = sqlReader["ObjectName"].ToString();
            detail.FullPath = sqlReader["FullPath"].ToString();
            detail.ItemType = JobReportUtility.ConvertStringForDetails(sqlReader["ItemType"].ToString(), isMergeRpt);
            detail.Action = JobReportUtility.ConvertStringForDetails(sqlReader["Action"].ToString(), isMergeRpt);
            detail.Comment = JobReportUtility.ConvertStringForDetails(sqlReader["Comment"].ToString(), isMergeRpt);
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            try
            {
                detail.AgentName = sqlReader["AgentName"]?.ToString();
            }
            catch(Exception e)
            {
                logger.Error($"An error occurred while get sync local node job detail of agent name. Error: {e}");
            }
            return detail;
        }

        public JMPhysicalDisposalJobDetails ConvertDomainToPhysicalDisposalDetails(SQLiteDataReader sqlReader, bool isMergeRpt)
        {
            JMPhysicalDisposalJobDetails detail = new JMPhysicalDisposalJobDetails();
            detail.ObjectName = sqlReader["ObjectName"].ToString();
            detail.ItemType = isMergeRpt ? sqlReader["ItemType"].ToString() : ConvertToI18NString(sqlReader["ItemType"].ToString());
            detail.FullPath = JobReportUtility.ReplaceRootLocationName(sqlReader["FullPath"].ToString());
            detail.RuleName = isMergeRpt ? sqlReader["RuleName"].ToString() : ConvertToI18NString(sqlReader["RuleName"].ToString());
            detail.ActionType = isMergeRpt ? sqlReader["ActionType"].ToString() : ConvertToI18NString(sqlReader["ActionType"].ToString());
            detail.DestinationPath = JobReportUtility.ReplaceRootLocationName(sqlReader["DestinationPath"].ToString());
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = isMergeRpt ? comment : I18NEntity.GetStringWithSeparator(comment);
            }
            return detail;
        }
        public JMWorkspaceHoldImportJobDetail ConvertDomainToJMWorkspaceHoldImportJobDetails(SQLiteDataReader sqlReader, bool isMergeRpt)
        {
            JMWorkspaceHoldImportJobDetail detail = new JMWorkspaceHoldImportJobDetail();
            detail.Url = sqlReader["Url"].ToString();
            detail.Type = sqlReader["Type"].ToString();
            detail.HoldTitle = sqlReader["HoldTitle"].ToString();
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = isMergeRpt ? comment : I18NEntity.GetStringWithSeparator(comment);
            }
            return detail;
        }

        public JMPhysicalExplorerTimerJobDetails ConvertDomainToPhysicalExplorerTimerDetails(SQLiteDataReader sqlReader, bool isMergeRpt)
        {
            JMPhysicalExplorerTimerJobDetails detail = new JMPhysicalExplorerTimerJobDetails();
            detail.ObjectName = sqlReader["ObjectName"].ToString();
            detail.ItemType = isMergeRpt ? sqlReader["ItemType"].ToString() : ConvertToI18NString(sqlReader["ItemType"].ToString());
            detail.FullPath = JobReportUtility.ReplaceRootLocationName(sqlReader["FullPath"].ToString());
            detail.RuleName = sqlReader["RuleName"].ToString();
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = isMergeRpt ? comment : I18NEntity.GetString(comment);
            }
            return detail;
        }

        public JMConnectorTimerJobDetails ConvertDomainToConnectorExplorerTimerDetails(SQLiteDataReader sqlReader, bool isMergeRpt)
        {
            JMConnectorTimerJobDetails detail = new JMConnectorTimerJobDetails();
            detail.ObjectName = sqlReader["ObjectName"].ToString();
            detail.TermName = sqlReader["TermName"].ToString();
            detail.ConnectorName = sqlReader["FullPath"].ToString();
            detail.RuleName = sqlReader["RuleName"].ToString();
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = isMergeRpt ? comment : I18NEntity.GetString(comment);
            }
            return detail;
        }

        public JMExportBarcodeJobDetail ConvertDomainToExportBarcodeToLocationDetails(SQLiteDataReader sqlReader, bool isMergeRpt)
        {
            JMExportBarcodeJobDetail detail = new JMExportBarcodeJobDetail();
            detail.ObjectName = sqlReader["ObjectName"].ToString();
            detail.ItemType = isMergeRpt ? sqlReader["ItemType"].ToString() : ConvertToI18NString(sqlReader["ItemType"].ToString());
            detail.FullPath = JobReportUtility.ReplaceRootLocationName(sqlReader["FullPath"].ToString());
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = isMergeRpt ? comment : I18NEntity.GetString(comment);
            }
            return detail;
        }

        public JMExportBarcodeJobDetail ConvertDomainToPhysicalSetPermissionDetails(SQLiteDataReader sqlReader, bool isMergeRpt)
        {
            JMExportBarcodeJobDetail detail = new JMExportBarcodeJobDetail();
            detail.ObjectName = sqlReader["ObjectName"].ToString();
            detail.ItemType = isMergeRpt ? sqlReader["ItemType"].ToString() : ConvertToI18NString(sqlReader["ItemType"].ToString());
            detail.FullPath = isMergeRpt ? sqlReader["FullPath"].ToString() : JobReportUtility.ReplaceRootLocationName(sqlReader["FullPath"].ToString());
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = isMergeRpt ? comment : I18NEntity.GetString(comment);
            }
            return detail;
        }

        public JMFSDashBoardJobDetail ConvertDomainToFSDashBoardDetails(SQLiteDataReader sqlReader, bool isMergeRpt)
        {
            JMFSDashBoardJobDetail detail = new JMFSDashBoardJobDetail();
            detail.Action = isMergeRpt ? sqlReader["Action"].ToString() : ConvertToI18NString(sqlReader["Action"].ToString());
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = isMergeRpt ? comment : I18NEntity.GetString(comment);
            }
            return detail;
        }

        public JMSPOnPremDashBoardJobDetail ConvertDomainToSPOnPremDashBoardDetails(SQLiteDataReader sqlReader, bool isMergeRpt)
        {
            var comment = sqlReader["Comment"].ToString();
            return new JMSPOnPremDashBoardJobDetail
            {
                Action = isMergeRpt ? sqlReader["Action"].ToString() : ConvertToI18NString(sqlReader["Action"].ToString()),
                Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString()),
                Comment = !string.IsNullOrEmpty(comment) && isMergeRpt ? comment : I18NEntity.GetString(comment)
            };
        }

        public JMDashboardJobDetail ConvertDomainToDashboardDetails(SQLiteDataReader sqlReader, bool isMergeRpt)
        {
            var comment = sqlReader["Comment"].ToString();
            return new JMDashboardJobDetail
            {
                Action = isMergeRpt ? sqlReader["Action"].ToString() : ConvertToI18NString(sqlReader["Action"].ToString()),
                SourceFlag = ConvertToI18NString(sqlReader["SourceFlag"].ToString()),
                Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString()),
                Comment = !string.IsNullOrEmpty(comment) && isMergeRpt ? comment : I18NEntity.GetString(comment)
            };
        }

        public JMTenantUpgradeDetails ConvertDomainToTenantUpgradeDetails(SQLiteDataReader sqlReader, bool isMergeRpt)
        {
            var comment = sqlReader["Comment"].ToString();
            return new JMTenantUpgradeDetails
            {
                UpgradeModule = isMergeRpt ? sqlReader["UpgradeModule"].ToString() : ConvertToI18NString(sqlReader["UpgradeModule"].ToString()),
                Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString()),
                Comment = !string.IsNullOrEmpty(comment) && isMergeRpt ? comment : I18NEntity.GetString(comment)
            };
        }

        public JMManualApprovalSettingScheduleDetail ConvertDomainToManualApprovalSettingScheduleDetail(SQLiteDataReader sqlReader, bool isMergeRpt)
        {
            var comment = sqlReader["Comment"].ToString();
            return new JMManualApprovalSettingScheduleDetail
            {
                TitleOrName = isMergeRpt ? sqlReader["TitleOrName"].ToString() : ConvertToI18NString(sqlReader["TitleOrName"].ToString()),
                Action = isMergeRpt ? sqlReader["Action"].ToString() : ConvertToI18NString(sqlReader["Action"].ToString()),
                Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString()),
                Comment = !string.IsNullOrEmpty(comment) && isMergeRpt ? comment : I18NEntity.GetString(comment)
            };
        }
        public JMFSDisposalJobDetails ConvertDomainToFSDisposalDetails(SQLiteDataReader sqlReader, bool isMergeRpt)
        {
            JMFSDisposalJobDetails detail = new JMFSDisposalJobDetails();
            //detail.DetailTab = sqlReader["DetailTab"].ToString();
            detail.Type = isMergeRpt ? sqlReader["Type"].ToString() : I18NEntity.GetString(sqlReader["Type"].ToString());
            detail.ObjectName = sqlReader["ObjectName"].ToString();
            detail.Size = sqlReader["Size"].ToString();
            detail.SourceLocation = sqlReader["SourceLocation"].ToString();
            detail.DestinationLocation = sqlReader["DestinationLocation"].ToString();
            detail.FinishTime = ConvertDetailsTimeZone(sqlReader["FinishTime"].ToString());
            detail.RuleName = sqlReader["RuleName"].ToString();
            detail.Action = isMergeRpt ? sqlReader["Action"].ToString() : I18NEntity.GetString(sqlReader["Action"].ToString());
            detail.AgentName = sqlReader["AgentName"].ToString();
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = isMergeRpt ? comment : I18NEntity.GetStringWithSeparator(comment);
            }
            return detail;
        }
        public JMFSRestoreJobDetails ConvertDomainToFSRestoreDetails(SQLiteDataReader sqlReader, bool isMergeRpt, GeneralSettingModel generalSettingModel)
        {
            JMFSRestoreJobDetails detail = new JMFSRestoreJobDetails();
            //detail.DetailTab = sqlReader["DetailTab"].ToString();
            detail.Size = sqlReader["Size"].ToString();
            detail.SizeStr = isMergeRpt ? "" : ConvertToFormatSize(long.Parse(sqlReader["Size"].ToString()));
            detail.SourceLocation = sqlReader["SourceLocation"].ToString();
            detail.FinishTime = sqlReader["FinishTime"] == null ? 0 : long.Parse(sqlReader["FinishTime"].ToString());
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            if (generalSettingModel != null)
            {
                detail.FinishTimeStr = isMergeRpt ? "" : GeneralSettingService.ConvertTiksToDateTime(generalSettingModel, detail.FinishTime, true).SimplifyFormatTime;
            }
            else
            {
                detail.FinishTimeStr = isMergeRpt ? "" : detail.FinishTime.ToString();
            }
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = isMergeRpt ? comment : I18NEntity.GetStringWithSeparator(comment);
            }
            return detail;
        }

        public FSDataSyncJobReportDetail ConvertDomainToFSDataSyncDetails(SQLiteDataReader sqlReader, bool isMergeRpt)
        {
            FSDataSyncJobReportDetail detail = new FSDataSyncJobReportDetail();
            detail.FullPath = sqlReader["FullPath"].ToString();
            //detail.Type = sqlReader["Type"].ToString();
            detail.ObjectName = sqlReader["ObjectName"].ToString();
            detail.AgentName = sqlReader["AgentName"].ToString();
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = isMergeRpt ? comment : I18NEntity.GetStringWithSeparator(comment);
            }
            return detail;
        }
        public FSDataSyncJobReportDetail ConvertDomainToFSImportSettingDetails(SQLiteDataReader sqlReader, bool isMergeRpt)
        {
            FSDataSyncJobReportDetail detail = new FSDataSyncJobReportDetail();
            detail.FullPath = sqlReader["Url"].ToString();
            //detail.Type = sqlReader["Type"].ToString();
            detail.ObjectName = sqlReader["ObjectName"].ToString();

            //detail.AgentName = sqlReader["AgentName"].ToString();
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = isMergeRpt ? comment : I18NEntity.GetStringWithSeparator(comment);
            }
            return detail;
        }

        public JMFSReclassifierJobDetails ConvertDomainToFSFolderReclassifyDetails(SQLiteDataReader sqlReader, bool isMergeRpt)
        {
            JMFSReclassifierJobDetails detail = new JMFSReclassifierJobDetails();
            detail.ObjectName = sqlReader["ObjectName"].ToString();
            detail.FullPath = sqlReader["FullPath"].ToString();
            detail.ItemType = sqlReader["ItemType"] == null ? 0 : Int32.Parse(sqlReader["ItemType"].ToString());
            detail.FinishTime = sqlReader["FinishTime"] == null ? 0 : long.Parse(sqlReader["FinishTime"].ToString());
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = isMergeRpt ? comment : I18NEntity.GetStringWithSeparator(comment);
            }
            return detail;
        }

        public JMFSHoldJobDetails ConvertDomainToFSFolderHoldDetails(SQLiteDataReader sqlReader, bool isMergeRpt)
        {
            JMFSHoldJobDetails detail = new JMFSHoldJobDetails();
            detail.ObjectName = sqlReader["ObjectName"].ToString();
            detail.FullPath = sqlReader["FullPath"].ToString();
            detail.Action = isMergeRpt ? sqlReader["Action"].ToString() : I18NEntity.GetString(sqlReader["Action"].ToString());
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = isMergeRpt ? comment : I18NEntity.GetStringWithSeparator(comment);
            }
            return detail;
        }

        public JMGlobalSearchActionJobDetails ConvertDomainToGlobalSearchActionDetails(SQLiteDataReader sqlReader, bool isMergeRpt)
        {
            JMGlobalSearchActionJobDetails detail = new JMGlobalSearchActionJobDetails();
            detail.ObjectName = sqlReader["ObjectName"].ToString();
            detail.Type = isMergeRpt ? sqlReader["Type"].ToString() : I18NEntity.GetString(sqlReader["Type"].ToString());
            detail.FullPath = JobReportUtility.ReplaceRootLocationName(sqlReader["FullPath"].ToString());
            detail.DestinationLocation= sqlReader["DestinationLocation"].ToString();
            detail.Action = isMergeRpt ? sqlReader["Action"].ToString() : I18NEntity.GetString(sqlReader["Action"].ToString());
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = isMergeRpt ? comment : I18NEntity.GetStringWithSeparator(comment);
            }
            return detail;
        }
        public JMPhyBoxLoanJobDetails ConvertDomainToPhyLoanBoxDetails(SQLiteDataReader sqlReader, bool isMergeRpt)
        {
            JMPhyBoxLoanJobDetails detail = new JMPhyBoxLoanJobDetails();
            detail.Name = sqlReader["Name"].ToString();
            detail.Level = isMergeRpt ? sqlReader["Level"].ToString() : I18NEntity.GetString(sqlReader["Level"].ToString());
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = isMergeRpt ? comment : I18NEntity.GetStringWithSeparator(comment);
            }
            return detail;
        }
        public JMPhysicalMoveJobDetails ConvertDomainToPhyMoveDetails(SQLiteDataReader sqlReader, bool isMergeRpt)
        {
            JMPhysicalMoveJobDetails detail = new JMPhysicalMoveJobDetails();
            detail.ObjectName = sqlReader["ObjectName"].ToString();
            detail.UniqueId = sqlReader["UniqueId"].ToString();
            detail.ItemType = isMergeRpt ? sqlReader["ItemType"].ToString() : I18NEntity.GetString(sqlReader["ItemType"].ToString());
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = isMergeRpt ? comment : I18NEntity.GetStringWithSeparator(comment);
            }
            return detail;
        }

        public JMClientAuditReportJobDetails ConvertDomainToAuditReportDetails(SQLiteDataReader sqlReader, bool isMergeRpt)
        {
            JMClientAuditReportJobDetails detail = new JMClientAuditReportJobDetails();
            detail.Type = sqlReader["Type"].ToString();
            detail.ObjectPath = sqlReader["ObjectPath"].ToString();
            detail.Count = sqlReader["Count"].ToString();
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = isMergeRpt ? comment : I18NEntity.GetStringWithSeparator(comment);
            }
            return detail;
        }


        public JMArchiverActionJobDetails ConvertDomainToArchiverActionReportDetails(SQLiteDataReader sqlReader, bool isMergeRpt, GeneralSettingModel generalSettingModel, int jobType)
        {
            JMArchiverActionJobDetails detail = new JMArchiverActionJobDetails();
            detail.ActionTab = Int32.Parse(sqlReader["ActionTab"].ToString());
            detail.Level = isMergeRpt ? sqlReader["Level"].ToString() : I18NEntity.GetString(sqlReader["Level"].ToString());
            detail.Size = sqlReader["Size"].ToString();
            detail.FinishTime = sqlReader["FinishTime"] == null ? 0 : long.Parse(sqlReader["FinishTime"].ToString());
            if(sqlReader["Level"].ToString().Equals("RM_Archiver_JobDetailTeamsGroupLevel")) 
            {
                detail.SizeStr = isMergeRpt ? "" : detail.Size;
            }
            else
            {
                detail.SizeStr = isMergeRpt ? "" : ConvertToFormatSize(long.Parse(detail.Size));
            }
            if (generalSettingModel != null)
            {
                detail.FinishTimeStr = isMergeRpt ? "" : GeneralSettingService.ConvertTiksToDateTime(generalSettingModel, detail.FinishTime, true).SimplifyFormatTime;
            }
            else
            {
                detail.FinishTimeStr = isMergeRpt ? "" : detail.FinishTime.ToString();
            }
            detail.SourceLocation = isMergeRpt ? sqlReader["SourceLocation"].ToString() : JobReportUtility.ReplaceRootLocationName(sqlReader["SourceLocation"].ToString());
            detail.DestinationLocation = sqlReader["DestinationLocation"].ToString();
            detail.Action = isMergeRpt ? sqlReader["Action"].ToString() : I18NEntity.GetString(sqlReader["Action"].ToString());

            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = jobType == (int)JobType.DiscoveryAOSPOptimization || isMergeRpt ? comment : I18NEntity.GetMultiStringWithSeparator(comment);
            }
            detail.RuleName = sqlReader["RuleName"].ToString();

            if (jobType == (int)JobType.SOPreScan || jobType == (int)JobType.TeamsPreScan || jobType == (int)JobType.DiscoveryPreScan)
            {
                ConvertDomainToArchiverActionForSimulate(detail, sqlReader, isMergeRpt, generalSettingModel);
            }   
            return detail;
        }

        private void ConvertDomainToArchiverActionForSimulate(JMArchiverActionJobDetails detail, SQLiteDataReader sqlReader, bool isMergeRpt, GeneralSettingModel generalSettingModel)
        {
            try
            {
                HashSet<string> allowedLevels = new HashSet<string> 
                {
                    I18NEntity.GetString("RM_JS_Rule_ObjectLevel_Item"),
                    I18NEntity.GetString("RM_JS_Rule_ObjectLevel_ItemVersion"),
                    I18NEntity.GetString("RM_JS_Rule_ObjectLevel_Attachment"),
                };
                if (allowedLevels.Contains(detail.Level))
                {
                    detail.CreatedBy = sqlReader["CreatedBy"] == null ? "" : sqlReader["CreatedBy"].ToString();
                    detail.Created = sqlReader["CreatedDate"] == null ? 0 : long.Parse(sqlReader["CreatedDate"].ToString());
                    detail.CreatedTime = isMergeRpt || detail.Created == 0 ? "" : GeneralSettingService.ConvertTiksToDateTime(generalSettingModel, detail.Created, true).SimplifyFormatTime;
                    detail.ModifiedBy = sqlReader["ModifiedBy"] == null ? "" : sqlReader["ModifiedBy"].ToString();
                    detail.Modified = sqlReader["ModifiedDate"] == null ? 0 : long.Parse(sqlReader["ModifiedDate"].ToString());
                    detail.ModifiedTime = isMergeRpt || detail.Modified == 0 ? "" : GeneralSettingService.ConvertTiksToDateTime(generalSettingModel, detail.Modified, true).SimplifyFormatTime;
                    detail.RuleMatchFile = sqlReader["RuleMatchFile"] == null ? "" : sqlReader["RuleMatchFile"].ToString();
                }
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while get archiver action job detail of simulate. Error: {e}");
                logger.Warn("this detail of the old job, maybe have not CreatedBy/CreatedDate/ModifiedBy/ModifiedDate/RuleMatchFile paramater.");
            }
        }

        public JMArchiverDedupJobDetails ConvertDomainToArchiverDedupReportDetails(SQLiteDataReader sqlReader, bool isMergeRpt, GeneralSettingModel generalSettingModel)
        {
            JMArchiverDedupJobDetails detail = new JMArchiverDedupJobDetails();
            detail.DedupTime = sqlReader["Date"] == null ? 0 : long.Parse(sqlReader["Date"].ToString());
            detail.Size = sqlReader["Size"] == null ? 0 : long.Parse(sqlReader["Size"].ToString());
            detail.SizeStr = isMergeRpt ? "" : ConvertToFormatSize(detail.Size);
            detail.SrcURL = sqlReader["SrcURL"].ToString();
            detail.SubJobId = sqlReader["SubJobId"].ToString();
            detail.Name = sqlReader["Remark9"].ToString();
            detail.ModifyTime = sqlReader["Remark10"] == null ? 0 : long.Parse(sqlReader["Remark10"].ToString());
            detail.BackupSubJobId = sqlReader["Remark11"].ToString();
            detail.NewFileStoragePath = sqlReader["Remark12"].ToString();
            detail.OldFileStoragePath = sqlReader["Remark13"].ToString();

            if (generalSettingModel != null)
            {
                detail.DedupTimeStr = isMergeRpt ? "" : GeneralSettingService.ConvertTiksToDateTime(generalSettingModel, detail.DedupTime, true).SimplifyFormatTime;
                detail.ModifyTimeStr = isMergeRpt ? "" : GeneralSettingService.ConvertTiksToDateTime(generalSettingModel, detail.ModifyTime, true).SimplifyFormatTime;
            }
            else
            {
                detail.DedupTimeStr = isMergeRpt ? "" : detail.DedupTime.ToString();
                detail.ModifyTimeStr = isMergeRpt ? "" : detail.ModifyTime.ToString();
            }

            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());

            if(detail.SubJobId?.StartsWith("DD") != true)
            {
                string comment = sqlReader["Comment"].ToString();
                if (!string.IsNullOrEmpty(comment))
                {
                    detail.Comment = isMergeRpt ? comment : I18NEntity.GetStringWithSeparator(comment);
                }
            }
            
            return detail;
        }

        public JMGDriveRestoreActionJobDetail ConvertDomainToGDriveRestoreActionReportDetails(SQLiteDataReader sqlReader, bool isMergeRpt, GeneralSettingModel generalSettingModel, int jobType)
        {
            JMGDriveRestoreActionJobDetail detail = new JMGDriveRestoreActionJobDetail();
            detail.Level = isMergeRpt ? sqlReader["Level"].ToString() : I18NEntity.GetString(sqlReader["Level"].ToString());
            detail.Size = sqlReader["Size"].ToString();
            detail.SizeStr = isMergeRpt ? "" : ConvertToFormatSize(long.Parse(detail.Size));
            detail.FinishTime = sqlReader["FinishTime"] == null ? 0 : long.Parse(sqlReader["FinishTime"].ToString());
            if (generalSettingModel != null)
            {
                detail.FinishTimeStr = isMergeRpt ? "" : GeneralSettingService.ConvertTiksToDateTime(generalSettingModel, detail.FinishTime, true).SimplifyFormatTime;
            }
            else
            {
                detail.FinishTimeStr = isMergeRpt ? "" : detail.FinishTime.ToString();
            }
            detail.SourceLocation = sqlReader["SourceLocation"].ToString();
            try
            {
                detail.Path = sqlReader["Path"].ToString();//Compatible with old data job detail
            }
            catch (Exception e)
            {
                logger.Warn("this detail is old,have not Path paramater");
                detail.Path = detail.SourceLocation;
            }
            var columns = Enumerable.Range(0, sqlReader.FieldCount)
                        .Select(sqlReader.GetName)
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);
            if (columns.Contains("DriveId"))
            {
                detail.DriveId = sqlReader["DriveId"].ToString();
            }
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = isMergeRpt ? comment : I18NEntity.GetStringWithSeparator(comment);
            }
            return detail;
        }

        public JMRestoreActionJobDetailes ConvertDomainToRestoreActionReportDetails(SQLiteDataReader sqlReader, bool isMergeRpt, GeneralSettingModel generalSettingModel, int jobType)
        {
            JMRestoreActionJobDetailes detail = new JMRestoreActionJobDetailes();
            detail.Level = isMergeRpt ? sqlReader["Level"].ToString() : I18NEntity.GetString(sqlReader["Level"].ToString());
            detail.Size = sqlReader["Size"].ToString();
            detail.SizeStr = isMergeRpt ? "" : ConvertToFormatSize(long.Parse(detail.Size));
            detail.FinishTime = sqlReader["FinishTime"] == null ? 0 : long.Parse(sqlReader["FinishTime"].ToString());
            if (generalSettingModel != null)
            {
                detail.FinishTimeStr = isMergeRpt ? "" : GeneralSettingService.ConvertTiksToDateTime(generalSettingModel, detail.FinishTime, true).SimplifyFormatTime;
            }
            else
            {
                detail.FinishTimeStr = isMergeRpt ? "" : detail.FinishTime.ToString();
            }
            detail.SourceLocation = sqlReader["SourceLocation"].ToString();
            try
            {
                detail.Path = sqlReader["Path"].ToString();//Compatible with old data job detail
            }
            catch (Exception e)
            {
                logger.Warn("this detail is old,have not Path paramater");
                detail.Path = detail.SourceLocation;
            }
            var columns = Enumerable.Range(0, sqlReader.FieldCount)
                        .Select(sqlReader.GetName)
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);
            if (columns.Contains("ConflictResolution"))
            {
                _ = int.TryParse(sqlReader["ConflictResolution"].ToString(), out var result);
                detail.ConflictResolution = result;
            }
            if (columns.Contains("PathMd5"))
            {
                detail.PathMd5 = sqlReader["PathMd5"].ToString();
            }
            if (columns.Contains("PolicyLevel"))
            {
                detail.PolicyLevel = sqlReader["PolicyLevel"].ToString();
            }
            if (columns.Contains("DestinationUrl"))
            {
                detail.DestinationUrl = sqlReader["DestinationUrl"].ToString();
            }
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = jobType == (int)JobType.AOSPRestore || isMergeRpt ? comment : I18NEntity.GetStringWithSeparator(comment);
            }
            return detail;
        }
        public JMVEOMergeJobDetails ConvertDomainToVEOMergeDetails(SQLiteDataReader sqlReader, bool isMergeRpt, GeneralSettingModel generalSettingModel)
        {
            JMVEOMergeJobDetails detail = new JMVEOMergeJobDetails();
            detail.Size = sqlReader["Size"].ToString();
            detail.SizeStr = isMergeRpt ? "" : ConvertToFormatSize(long.Parse(detail.Size));
            detail.FinishTime = sqlReader["FinishTime"] == null ? 0 : long.Parse(sqlReader["FinishTime"].ToString());
            if (generalSettingModel != null)
            {
                detail.FinishTimeStr = isMergeRpt ? "" : GeneralSettingService.ConvertTiksToDateTime(generalSettingModel, detail.FinishTime, true).SimplifyFormatTime;
            }
            else
            {
                detail.FinishTimeStr = isMergeRpt ? "" : detail.FinishTime.ToString();
            }
            detail.SourceLocation = sqlReader["SourceLocation"].ToString();
            detail.FileName = sqlReader["FileName"].ToString();
            detail.DestinationLocation = sqlReader["DestinationLocation"].ToString();
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = isMergeRpt ? comment : I18NEntity.GetStringWithSeparator(comment);
            }
            return detail;
        }
        public JMArchiverMoveIndexJobDetails ConvertDomainToMergeIndexReportDetails(SQLiteDataReader sqlReader, bool isMergeRpt, GeneralSettingModel generalSettingModel)
        {
            JMArchiverMoveIndexJobDetails detail = new JMArchiverMoveIndexJobDetails();
            detail.SiteUrl = sqlReader["SiteCollectionURL"].ToString();
            detail.Size = sqlReader["Size"].ToString();
            detail.SizeStr = isMergeRpt ? "" : ConvertToFormatSize(long.Parse(detail.Size));
            detail.SrcStorageName = sqlReader["SourceLocation"].ToString();
            detail.DesStorageName = sqlReader["DestinationLocation"].ToString();
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = isMergeRpt ? comment : I18NEntity.GetStringWithSeparator(comment);
            }
            return detail;
        }

        public JMArchiverRententionJobDetails ConvertDomainToRetentionReportDetails(SQLiteDataReader sqlReader, bool isMergeRpt, GeneralSettingModel generalSettingModel)
        {
            JMArchiverRententionJobDetails detail = new JMArchiverRententionJobDetails();
            detail.SiteUrl = sqlReader["SiteCollectionURL"].ToString();
            detail.JobId = sqlReader["JobId"].ToString();
            detail.Size = sqlReader["Size"].ToString();
            detail.SizeStr = isMergeRpt ? "" : string.IsNullOrEmpty(detail.Size)?"": ConvertToFormatSize(long.Parse(detail.Size));
            detail.SrcStorageName = sqlReader["SourceLocation"].ToString();
            detail.DesStorageName = sqlReader["DestinationLocation"].ToString();
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            detail.Action = sqlReader.FieldCount == 8?"":sqlReader["Action"] == null ?string.Empty : JobReportUtility.ConvertStringForDetails(sqlReader["Action"].ToString(), isMergeRpt);//兼容老数据
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = isMergeRpt ? comment : I18NEntity.GetStringWithSeparator(comment);
            }
            return detail;
        }

        public JMArchiverRententionDashboardDetails ConvertDomainToRetentionDashboardReportDetails(SQLiteDataReader sqlReader, bool isMergeRpt, GeneralSettingModel generalSettingModel)
        {
            JMArchiverRententionJobDetails temp = ConvertDomainToRetentionReportDetails(sqlReader, isMergeRpt, generalSettingModel);
            JMArchiverRententionDashboardDetails detail = new JMArchiverRententionDashboardDetails(temp);
            detail.SourceFlag = sqlReader["SourceFlag"] == null ? 0 : Convert.ToInt32(sqlReader["SourceFlag"].ToString());
            detail.FileName = sqlReader["FileName"]?.ToString();
            detail.RetentionSource = sqlReader["RetentionSource"]?.ToString();
            detail.RetentionKeepDate = sqlReader["RetentionKeepDate"] == null ? 0 : Convert.ToInt32(sqlReader["RetentionKeepDate"].ToString());
            detail.RetentionKeepDateUnit = sqlReader["RetentionKeepDateUnit"] == null ? 0 : Convert.ToInt32(sqlReader["RetentionKeepDateUnit"].ToString());
            return detail;
        }

        public JMDeleteOrphanDatasJobDetails ConvertDomainToOrphanDatasReportDetails(SQLiteDataReader sqlReader, bool isMergeRpt, GeneralSettingModel generalSettingModel)
        {
            JMDeleteOrphanDatasJobDetails detail = new JMDeleteOrphanDatasJobDetails();
            detail.SiteUrl = sqlReader["SiteCollectionURL"].ToString();
            detail.JobId = sqlReader["JobId"].ToString();
            detail.Size = sqlReader["Size"].ToString();
            if (string.IsNullOrEmpty(detail.Size))
            {
                detail.Size = "0";
            }
            detail.SizeStr = isMergeRpt ? "" : ConvertToFormatSize(long.Parse(detail.Size));
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = isMergeRpt ? comment : I18NEntity.GetStringWithSeparator(comment);
            }
            return detail;
        }
        public JMArchiverRebuildStubJobDetails ConvertDomainToRebuildStubReportDetails(SQLiteDataReader sqlReader, bool isMergeRpt)
        {
            JMArchiverRebuildStubJobDetails detail = new JMArchiverRebuildStubJobDetails();
            detail.SiteUrl = sqlReader["SiteCollectionURL"].ToString();
            detail.JobId = sqlReader["JobId"].ToString();
            detail.StubUrl = sqlReader["SourceLocation"].ToString();
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = isMergeRpt ? comment : I18NEntity.GetStringWithSeparator(comment);
            }
            return detail;
        }

        public JMArchiverRebuildIndexJobDetails ConvertDomainToRebuildIndexReportDetails(SQLiteDataReader sqlReader, bool isMergeRpt)
        {
            JMArchiverRebuildIndexJobDetails detail = new JMArchiverRebuildIndexJobDetails();
            detail.SiteUrl = sqlReader["SiteUrl"].ToString();
            detail.ObjectUrl = sqlReader["ObjectUrl"]?.ToString();
            detail.ObjectType = sqlReader["ObjectType"]?.ToString();
            detail.JobId = sqlReader["JobId"]?.ToString();
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = isMergeRpt ? comment : I18NEntity.GetStringWithSeparator(comment);
            }
            return detail;
        }

        public JMArchiverMigrationJobDetails ConvertDomainToArchiverMigrationJobDetails(SQLiteDataReader sqlReader, bool isMergeRpt, GeneralSettingModel generalSettingModel)
        {
            JMArchiverMigrationJobDetails detail = new JMArchiverMigrationJobDetails();
            detail.ObjectName = sqlReader["ObjectName"].ToString();
            detail.ObjectType = sqlReader["ObjectType"].ToString();
            detail.ObjectType = I18NEntity.GetString(detail.ObjectType);
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = isMergeRpt ? comment : I18NEntity.GetStringWithSeparator(comment);
            }
            return detail;
        }

        public JMExportSearchResultJobDetails ConvertDomainToExportSearchResultDetails(SQLiteDataReader sqlReader, bool isMergeRpt)
        {
            JMExportSearchResultJobDetails detail = new JMExportSearchResultJobDetails();
            detail.ExportLocation = sqlReader["ExportLocation"].ToString();
            detail.ReportName = sqlReader["ReportName"].ToString();
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = isMergeRpt ? comment : I18NEntity.GetStringWithSeparator(comment);
            }
            return detail;
        }

        public JMOnPremiseSPEnforceRuleActionJobDetails ConvertDomainToOnPremEnforceRuleActionDetails(SQLiteDataReader sqlReader, bool isMergeRpt)
        {
            JMOnPremiseSPEnforceRuleActionJobDetails detail = new JMOnPremiseSPEnforceRuleActionJobDetails();
            //detail.DetailTab = sqlReader["DetailTab"].ToString();
            detail.Type = isMergeRpt ? sqlReader["Type"].ToString() : I18NEntity.GetString(sqlReader["Type"].ToString());
            detail.ObjectName = sqlReader["ObjectName"].ToString();
            detail.Size = sqlReader["Size"].ToString();
            detail.SourceLocation = sqlReader["SourceLocation"].ToString();
            //detail.DestinationLocation = sqlReader["DestinationLocation"].ToString();
            detail.FinishTime = ConvertDetailsTimeZone(sqlReader["FinishTime"].ToString());
            detail.RuleName = sqlReader["RuleName"].ToString();
            detail.Action = isMergeRpt ? sqlReader["Action"].ToString() : I18NEntity.GetString(sqlReader["Action"].ToString());
            detail.AgentName = sqlReader["AgentName"].ToString();
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = isMergeRpt ? comment : I18NEntity.GetStringWithSeparator(comment);
            }
            return detail;
        }

        public JMPickCompleteJobDetails ConvertDomainToJMPickCompleteJobDetails(SQLiteDataReader sqlReader, bool isMergeRpt)
        {
            JMPickCompleteJobDetails detail = new JMPickCompleteJobDetails();
            detail.Name = sqlReader["Name"].ToString();
            detail.FullPath = sqlReader["FullPath"].ToString();
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = isMergeRpt ? comment : I18NEntity.GetStringWithSeparator(comment);
            }
            return detail;
        }

        public JMTrainingJobDetails ConvertDomainToJMTrainingJobDetails(SQLiteDataReader sqlReader, bool isMergeRpt)
        {
            JMTrainingJobDetails detail = new JMTrainingJobDetails();
            detail.TermName = sqlReader["TermName"].ToString();
            detail.FileName = sqlReader["Name"].ToString();
            detail.FullPath = sqlReader["FullPath"].ToString();
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = isMergeRpt ? comment : I18NEntity.GetStringWithSeparator(comment);
            }
            return detail;
        }

        private string ConvertJobDetailsStatusToString(JobDetailsStatus status)
        {
            string result = null;
            switch (status)
            {
                case JobDetailsStatus.Successful:
                    result = I18NEntity.GetString("RM_JS_JMD_Status_Successful");
                    break;
                case JobDetailsStatus.Failed:
                    result = I18NEntity.GetString("RM_JS_JMD_Status_Failed");
                    break;
                case JobDetailsStatus.Skipped:
                    result = I18NEntity.GetString("RM_JS_JMD_Status_Skipped");
                    break;
                case JobDetailsStatus.Pending:
                    result = I18NEntity.GetString("RM_JS_JMD_Status_Pending");
                    break;
            }
            return result;
        }

        public JMDisposalJobDetails ConvertDomainToJMDisposalJob(SQLiteDataReader sqlReader, GeneralSettingModel generalSettingModel, JobType jobType)
        {
            var itemType = sqlReader.GetString("Type");
            JMDisposalJobDetails detail = new JMDisposalJobDetails()
            {
                EntityType = sqlReader.GetInt32("EntityType"),
                Type = string.IsNullOrEmpty(itemType) ? I18NEntity.GetString("RM_Archiver_JobDetailExceptionLevel") : itemType,
                SourceURL = JobReportUtility.ReplaceRootLocationName(sqlReader.GetString("SrcURL")),
                SizeNumber = sqlReader.GetInt64("Size"),
                Status = (JobDetailsStatus)sqlReader.GetInt32("Status"),
                Date = sqlReader.GetInt64("Date"),
                FinishTime = GeneralSettingService.ConvertTiksToDateTime(generalSettingModel, sqlReader.GetInt64("Date"), true).SimplifyFormatTime,
                Action = sqlReader.GetValue("Remark11").ToString(),
                Comment = sqlReader.GetValue("Message").ToString()
            };

            if (!sqlReader.IsDBNull(sqlReader.GetOrdinal("DestURL")))
            {
                detail.DestinationURL = JobReportUtility.ReplaceRootLocationName(sqlReader.GetString("DestURL"));
            }
            else
            {
                detail.DestinationURL = "";
            }

            if (jobType == JobType.ArchiverScan || jobType == JobType.ExchangeArchiverScan)
            {
                detail.RuleName = sqlReader.GetValue("Remark3").ToString();
            }
            else if (jobType == JobType.ArchiverBackup || jobType == JobType.ExchangeArchiverBackup || jobType == JobType.MigrationArchiverBackup)
            {
                detail.RuleName = sqlReader.GetValue("Option").ToString();
            }
            detail.Action = I18NEntity.GetString(detail.Action);
            detail.StatusStr = ConvertJobDetailsStatusToString(detail.Status);
            return detail;
        }

        public JMArchiverMigrationRententionJobDetails ConvertDomainToMigrationRetentionReportDetails(SQLiteDataReader sqlReader, GeneralSettingModel generalSettingModel)
        {
            JMArchiverMigrationRententionJobDetails detail = new JMArchiverMigrationRententionJobDetails();
            detail.Action = sqlReader["Remark9"].ToString();
            detail.LogicalDevice = sqlReader["Remark7"].ToString();
            detail.MoveDataTo = sqlReader["Remark8"].ToString();
            detail.Size = sqlReader["Size"].ToString();
            detail.SizeStr = ConvertToFormatSize(long.Parse(detail.Size));
            detail.Date = GeneralSettingService.ConvertTiksToDateTime(generalSettingModel, sqlReader.GetInt64("Date"), true).SimplifyFormatTime;
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            string comment = sqlReader["Message"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = I18NEntity.GetStringWithSeparator(comment);
            }
            return detail;
        }

        public JMArchiverMigrationFileLevelRetentionJobDetails ConvertDomainToMigrationFileLevelRetentionReportDetails(SQLiteDataReader sqlReader, GeneralSettingModel generalSettingModel)
        {
            JMArchiverMigrationFileLevelRetentionJobDetails detail = new JMArchiverMigrationFileLevelRetentionJobDetails();
            var entityType = sqlReader.GetInt32("EntityType");
            var action = sqlReader.IsDBNull("Remark1") ? 0 : sqlReader.GetInt32("Remark1");
            var actionName = entityType switch
            {
                (int)JobReportDetailEntityType.FileRetention => I18NEntity.GetString("RM_Archiver_JobDetail_Type_FileRetention"),
                (int)JobReportDetailEntityType.RemoveStub => I18NEntity.GetString("RM_Archiver_JobDetail_Type_StubRemoval"),
                (int)JobReportDetailEntityType.ChangeFileTier => action == 0
                    ? I18NEntity.GetString("StorageOptimization.Service_b6bb8c3f-fe68-48a7-9f8e-17db0d7ddd48")  //Change Tier to Archive
                    : I18NEntity.GetString("StorageOptimization.Service_e2f7cb2c-1d94-42f5-a737-d9a28aa095b9"), //Change Tier to Cool
                _ => ""
            };
            detail.Action = actionName;
            detail.FileName = sqlReader.GetString("Remark9");
            detail.FilePath = sqlReader.GetString("SrcURL");
            detail.JobId = sqlReader.GetString("Remark11");
            detail.Size = sqlReader.GetInt64("Size");
            //detail.SizeStr = ConvertToFormatSize(detail.Size);
            detail.LastModified = sqlReader.IsDBNull("Remark10") ? 0 : sqlReader.GetInt64("Remark10");
            detail.LastModifiedStr = detail.LastModified == 0 ? "" : GeneralSettingService.ConvertTiksToDateTime(generalSettingModel, detail.LastModified, true).SimplifyFormatTime;
            detail.FinishTime = sqlReader.GetInt64("Date");
            detail.FinishTimeStr = GeneralSettingService.ConvertTiksToDateTime(generalSettingModel, detail.FinishTime, true).SimplifyFormatTime;
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            string comment = sqlReader["Message"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = I18NEntity.GetStringWithSeparator(comment);
            }
            return detail;
        }

        public JMPhysicalDisposalJobDetails ConvertDomainToMigrationPhysicalDisposalDetails(SQLiteDataReader sqlReader)
        {
            var itemType = sqlReader.GetString("Type");
            JMPhysicalDisposalJobDetails detail = new JMPhysicalDisposalJobDetails();
            detail.ObjectName = sqlReader["MediaHost"].ToString();
            detail.ItemType = string.IsNullOrEmpty(itemType) ? I18NEntity.GetString("RM_Archiver_JobDetailExceptionLevel") : itemType;
            detail.FullPath = JobReportUtility.ReplaceRootLocationName(sqlReader["SrcURL"].ToString());
            detail.RuleName = ConvertToI18NString(sqlReader["Remark3"].ToString());
            detail.ActionType = sqlReader["Remark11"].ToString();
            detail.DestinationPath = JobReportUtility.ReplaceRootLocationName(sqlReader["DestURL"].ToString());
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            string comment = sqlReader["Message"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = I18NEntity.GetStringWithSeparator(comment);
            }
            detail.ActionType = I18NEntity.GetString(detail.ActionType);
            detail.StatusStr = ConvertJobDetailsStatusToString(detail.Status);
            return detail;
        }
        public JMDownloadJobReport ConvertDomainToDownLoadDetails(SQLiteDataReader sqlReader)
        {
            JMDownloadJobReport detail = new JMDownloadJobReport();
            detail.JobId = sqlReader["JobId"].ToString();
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            string comment = sqlReader["Message"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = I18NEntity.GetStringWithSeparator(comment);
            }
            return detail;
        }
        public JMRestoreReportJobDetailes ConvertDomainToRestoreReportDetails(SQLiteDataReader sqlReader)
        {
            JMRestoreReportJobDetailes detail = new JMRestoreReportJobDetailes();
            detail.Level = I18NEntity.GetString(sqlReader["Level"].ToString());
            detail.Title = sqlReader["Title"].ToString();
            detail.Url = sqlReader["Url"].ToString();
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            return detail;
        }
        public JMRestoreScDetails ConvertDomainToJMSCRestoreDetails(SQLiteDataReader sqlReader)
        {
            JMRestoreScDetails sCRestoreDetails = new JMRestoreScDetails();
            sCRestoreDetails.Level = sqlReader["Level"].ToString();
            sCRestoreDetails.Name = sqlReader["Name"].ToString();
            sCRestoreDetails.SourceURL = sqlReader["SourceURL"].ToString();
            sCRestoreDetails.Size = sqlReader.GetInt64("Size");
            sCRestoreDetails.RestoreBy = sqlReader["RestoreBy"].ToString();
            sCRestoreDetails.JobId = sqlReader["JobId"].ToString();
            sCRestoreDetails.StartTime = sqlReader.GetInt64("StartTime");
            sCRestoreDetails.FinishTime = sqlReader.GetInt64("FinishTime");
            sCRestoreDetails.RestoreTo = sqlReader["RestoreTo"].ToString();
            sCRestoreDetails.IsDaoMigration = Convert.ToInt32(sqlReader["IsDaoMigration"].ToString());
            sCRestoreDetails.IsEndUserOpt = Convert.ToInt32(sqlReader["ISEndUserOpt"].ToString());
            sCRestoreDetails.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            sCRestoreDetails.Comment = sqlReader["Comment"].ToString();
            return sCRestoreDetails;
        }
        public JMRestoreGDriveDetails ConvertDomainToJMGDRestoreDetails(SQLiteDataReader sqlReader)
        {
            JMRestoreGDriveDetails gDriveRestoreDetails = new JMRestoreGDriveDetails();
            gDriveRestoreDetails.Level = sqlReader["Level"].ToString();
            gDriveRestoreDetails.Name = sqlReader["Name"].ToString();
            gDriveRestoreDetails.SourceURL = sqlReader["SourceURL"].ToString();
            gDriveRestoreDetails.Size = sqlReader.GetInt64("Size");
            gDriveRestoreDetails.RestoreBy = sqlReader["RestoreBy"].ToString();
            gDriveRestoreDetails.JobId = sqlReader["JobId"].ToString();
            gDriveRestoreDetails.StartTime = sqlReader.GetInt64("StartTime");
            gDriveRestoreDetails.FinishTime = sqlReader.GetInt64("FinishTime");
            gDriveRestoreDetails.RestoreTo = sqlReader["RestoreTo"].ToString();
            gDriveRestoreDetails.IsDaoMigration = Convert.ToInt32(sqlReader["IsDaoMigration"].ToString());
            gDriveRestoreDetails.IsEndUserOpt = Convert.ToInt32(sqlReader["ISEndUserOpt"].ToString());
            gDriveRestoreDetails.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            gDriveRestoreDetails.Comment = sqlReader["Comment"].ToString();
            return gDriveRestoreDetails;
        }

        public JMConvertStubJobDetails ConvertDomainToConvertStubJobDetails(SQLiteDataReader sqlReader, GeneralSettingModel generalSettingModel)
        {
            JMConvertStubJobDetails detail = new JMConvertStubJobDetails();
            detail.Action = Int32.Parse(sqlReader["Action"].ToString());
            var actionName = detail.Action switch
            {
                (int)ConvertStubAction.Scan => I18NEntity.GetString("RM_JM_JD_ConvertStub_Action_Scan"),
                (int)ConvertStubAction.Create => I18NEntity.GetString("RM_JM_JD_ConvertStub_Action_CreateNew"),
                (int)ConvertStubAction.Delete => I18NEntity.GetString("RM_JM_JD_ConvertStub_Action_DeleteOld"),
                _ => ""
            };
            detail.FullPath = sqlReader["FullPath"].ToString();
            detail.FinishTime = sqlReader["FinishTime"] == null ? 0 : long.Parse(sqlReader["FinishTime"].ToString());
            if (generalSettingModel != null)
            {
                detail.FinishTimeStr = GeneralSettingService.ConvertTiksToDateTime(generalSettingModel, detail.FinishTime, true).SimplifyFormatTime;
            }
            else
            {
                detail.FinishTimeStr = detail.FinishTime.ToString();
            }
            detail.ActionStr = actionName;
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = I18NEntity.GetString(comment);
            }
            return detail;
        }

        public JMDeclaredRecordsMigrationJobDetails ConvertDomainToDeclaredRecordsMigrationJobDetails(SQLiteDataReader sqlReader, GeneralSettingModel generalSettingModel)
        {
            JMDeclaredRecordsMigrationJobDetails detail = new JMDeclaredRecordsMigrationJobDetails
            {
                Url = sqlReader["Url"].ToString(),
                FinishTime = sqlReader["FinishTime"] == null ? 0 : long.Parse(sqlReader["FinishTime"].ToString())
            };

            if (generalSettingModel != null)
            {
                detail.FinishTimeStr = GeneralSettingService.ConvertTiksToDateTime(generalSettingModel, detail.FinishTime, true).SimplifyFormatTime;
            }
            else
            {
                detail.FinishTimeStr = detail.FinishTime.ToString();
            }
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = I18NEntity.GetString(comment);
            }
            return detail;
        }

        public JMDeleteArchivedSCJobDetails ConvertDomainToDeleteArchivedSCJobDetails(SQLiteDataReader sqlReader, bool isMergeRpt)
        {
            JMDeleteArchivedSCJobDetails detail = new ()
            {
                Url = sqlReader["Url"].ToString(),
                JobId = sqlReader["JobId"].ToString(),
                SourceStorageName = sqlReader["SourceStorageName"].ToString(),
                Size = sqlReader["Size"] == null ? 0 : long.Parse(sqlReader["Size"].ToString()),
            };

            detail.SizeStr = isMergeRpt ? "" : ConvertToFormatSize(detail.Size); // isMergeRpt ??
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = I18NEntity.GetString(comment);
            }
            return detail;
        }

        private JMJobDetails ConvertDomainToMultiGeoMainDCSyncCommonDataJobDetails(SQLiteDataReader sqlReader, bool isMergeRpt)
        {
            return new MainDataSyncCommonDataJobDetails();
        }

        private JMJobDetails ConvertDomainToMultiGeoOtherDCSyncCommonDataJobDetails(SQLiteDataReader sqlReader, bool isMergeRpt)
        {
            return new OtherDCSyncCommonDataJobDetails();
        }

        public JMStubDisposalJobDetails ConvertDomainToStubDisposalJobDetails(SQLiteDataReader sqlReader, GeneralSettingModel generalSettingModel)
        {
            JMStubDisposalJobDetails detail = new JMStubDisposalJobDetails
            {
                Url = sqlReader["Url"].ToString(),
                FinishTime = sqlReader["FinishTime"] == null ? 0 : long.Parse(sqlReader["FinishTime"].ToString())
            };

            if (generalSettingModel != null)
            {
                detail.FinishTimeStr = GeneralSettingService.ConvertTiksToDateTime(generalSettingModel, detail.FinishTime, true).SimplifyFormatTime;
            }
            else
            {
                detail.FinishTimeStr = detail.FinishTime.ToString();
            }
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = I18NEntity.GetString(comment);
            }
            return detail;
        }

        public JMConvertStubJobDetails ConvertDomainToConvertTeamsSettingUpgradeDetails(SQLiteDataReader sqlReader, GeneralSettingModel generalSettingModel)
        {
            JMConvertStubJobDetails detail = new JMConvertStubJobDetails();
            detail.Action = Int32.Parse(sqlReader["Action"].ToString());
            var actionName = I18NEntity.GetString("RM_JM_JD_TeamsUpgrade_Action_Upgrade");
            detail.FullPath = sqlReader["FullPath"].ToString();
            detail.FinishTime = sqlReader["FinishTime"] == null ? 0 : long.Parse(sqlReader["FinishTime"].ToString());
            if (generalSettingModel != null)
            {
                detail.FinishTimeStr = GeneralSettingService.ConvertTiksToDateTime(generalSettingModel, detail.FinishTime, true).SimplifyFormatTime;
            }
            else
            {
                detail.FinishTimeStr = detail.FinishTime.ToString();
            }
            detail.ActionStr = actionName;
            detail.Module = detail.Action == 0 ? I18NEntity.GetString("RM_JM_JD_TeamsUpgrade_Module_IL") : I18NEntity.GetString("RM_JM_JD_TeamsUpgrade_Module_SO");
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = I18NEntity.GetString(comment);
            }
            return detail;
        }

        public JMImportFullTextIndexSClistJobDetail ConvertDomainToImportSCWhitelistJobDetails(SQLiteDataReader sqlReader, GeneralSettingModel generalSettingModel)
        {
            JMImportFullTextIndexSClistJobDetail detail = new JMImportFullTextIndexSClistJobDetail();
            detail.Url = sqlReader["Url"].ToString();
            detail.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            string comment = sqlReader["Comment"].ToString();
            if (!string.IsNullOrEmpty(comment))
            {
                detail.Comment = I18NEntity.GetStringWithSeparator(comment);
            }
            return detail;
        }

        public static List<SQLiteParameter> BuildSQLiteParameters(JMJobDetails jobDetail)
        {
            List<SQLiteParameter> parameters = new List<SQLiteParameter>();
            if (jobDetail is JMReportJobDetails)
            {
                JMReportJobDetails detailInfo = jobDetail as JMReportJobDetails;
                parameters.Add(new SQLiteParameter("Type", detailInfo.Type));
                parameters.Add(new SQLiteParameter("TitleOrName", detailInfo.TitleOrName));
                parameters.Add(new SQLiteParameter("Url", detailInfo.Url));
            }
            else if (jobDetail is JMTermSyncJobDetails)
            {
                JMTermSyncJobDetails detailInfo = jobDetail as JMTermSyncJobDetails;
                parameters.Add(new SQLiteParameter("Term", detailInfo.Term));
                parameters.Add(new SQLiteParameter("Action", detailInfo.Action));
                //parameters.Add(new SQLiteParameter("SiteCollectionURL", detailInfo.SiteCollectionURL));
                parameters.Add(new SQLiteParameter("MMSApplication", detailInfo.MMSApplication));
                parameters.Add(new SQLiteParameter("AgentName", detailInfo.AgentName));
            }
            else if (jobDetail is JMTermSelection)
            {
                JMTermSelection detailInfo = jobDetail as JMTermSelection;
                parameters.Add(new SQLiteParameter("Term", detailInfo.Term));
                parameters.Add(new SQLiteParameter("TermFullPath", detailInfo.TermFullPath));
            }
            else if (jobDetail is JMGlobalSettingJobDetails)
            {
                JMGlobalSettingJobDetails detailInfo = jobDetail as JMGlobalSettingJobDetails;
                parameters.Add(new SQLiteParameter("ObjectName", detailInfo.ObjectName));
                parameters.Add(new SQLiteParameter("SourceURL", detailInfo.SourceURL));
                parameters.Add(new SQLiteParameter("ColumnName", detailInfo.ColumnName));
                parameters.Add(new SQLiteParameter("Action", detailInfo.Action));
                parameters.Add(new SQLiteParameter("Classification", detailInfo.Classification));
                parameters.Add(new SQLiteParameter("AgentName", detailInfo.AgentName));
            }
            else if (jobDetail is JMPhysicalSyncJobDetails)
            {
                JMPhysicalSyncJobDetails detailInfo = jobDetail as JMPhysicalSyncJobDetails;
                parameters.Add(new SQLiteParameter("SiteCollectionURL", detailInfo.SiteCollectionURL));
                parameters.Add(new SQLiteParameter("LocationPath", detailInfo.LocationPath));
                parameters.Add(new SQLiteParameter("TermName", detailInfo.TermName));
                parameters.Add(new SQLiteParameter("Action", detailInfo.Action));
                // parameters.Add(new SQLiteParameter("Classification", detailInfo.));
            }
            else if (jobDetail is JMUpdateLocationJobDetail)
            {
                JMUpdateLocationJobDetail detailInfo = jobDetail as JMUpdateLocationJobDetail;
                parameters.Add(new SQLiteParameter("SiteCollectionURL", detailInfo.SiteCollectionURL));
                parameters.Add(new SQLiteParameter("ItemType", detailInfo.ItemType));
                parameters.Add(new SQLiteParameter("SourceUrl", detailInfo.SourceUrl));
                parameters.Add(new SQLiteParameter("DestinationUrl", detailInfo.DestinationUrl));
            }
            else if (jobDetail is JMImportPhysicalRecordsJobDetail)
            {
                JMImportPhysicalRecordsJobDetail detailInfo = jobDetail as JMImportPhysicalRecordsJobDetail;
                parameters.Add(new SQLiteParameter("SrcRecordType", detailInfo.SrcRecordType));
                parameters.Add(new SQLiteParameter("DestRecordType", detailInfo.DestRecordType));
                parameters.Add(new SQLiteParameter("TemplateName", detailInfo.TemplateName));
                parameters.Add(new SQLiteParameter("UniqueId", detailInfo.UniqueId));
                parameters.Add(new SQLiteParameter("Title", detailInfo.Title));
                parameters.Add(new SQLiteParameter("Container", detailInfo.Container));
                parameters.Add(new SQLiteParameter("SrcLocation", detailInfo.SrcLocation));
                parameters.Add(new SQLiteParameter("LocationFullPath", detailInfo.LocationFullPath));
                parameters.Add(new SQLiteParameter("Barcode", detailInfo.Barcode));
            }
            else if (jobDetail is JMImportedPhysicalRecordsDeletionDetail)
            {
                JMImportedPhysicalRecordsDeletionDetail deletionDetail = jobDetail as JMImportedPhysicalRecordsDeletionDetail;
                parameters.Add(new SQLiteParameter("ObjectName", deletionDetail.ObjectName));
                parameters.Add(new SQLiteParameter("UniqueId", deletionDetail.UniqueId));
            }
            else if (jobDetail is JMImportRecordsRelatedJobDetail)
            {
                JMImportRecordsRelatedJobDetail detailInfo = jobDetail as JMImportRecordsRelatedJobDetail;
                parameters.Add(new SQLiteParameter("SrcId", detailInfo.SrcId));
                parameters.Add(new SQLiteParameter("SrcType", detailInfo.SrcType));
                parameters.Add(new SQLiteParameter("SrcName", detailInfo.SrcName));
                parameters.Add(new SQLiteParameter("SrcLocation", detailInfo.SrcLocation));
                parameters.Add(new SQLiteParameter("SrcSiteId", detailInfo.SrcSiteId));
                parameters.Add(new SQLiteParameter("SrcItemId", detailInfo.SrcItemId));
                parameters.Add(new SQLiteParameter("SrcItemUrl", detailInfo.SrcItemUrl));
                parameters.Add(new SQLiteParameter("DestType", detailInfo.DestType));
                parameters.Add(new SQLiteParameter("DestName", detailInfo.DestName));
                parameters.Add(new SQLiteParameter("DestItemId", detailInfo.DestItemId));
                parameters.Add(new SQLiteParameter("DestItemUrl", detailInfo.DestItemUrl));
                parameters.Add(new SQLiteParameter("DestSiteId", detailInfo.DestSiteId));
                parameters.Add(new SQLiteParameter("DestSiteUrl", detailInfo.DestSiteUrl));
            }
            else if (jobDetail is JMAvailableSpaceReportJobDetail)
            {
                JMAvailableSpaceReportJobDetail detailInfo = jobDetail as JMAvailableSpaceReportJobDetail;
                parameters.Add(new SQLiteParameter("Location", detailInfo.Location));
                parameters.Add(new SQLiteParameter("LocationSize", detailInfo.LocationSize));
            }
            else if (jobDetail is JMCreateAndDestroyedFileReportJobDetail)
            {
                JMCreateAndDestroyedFileReportJobDetail detailInfo = jobDetail as JMCreateAndDestroyedFileReportJobDetail;
                parameters.Add(new SQLiteParameter("ObjectLevel", detailInfo.ObjectLevel));
                parameters.Add(new SQLiteParameter("Title", detailInfo.Title));
                parameters.Add(new SQLiteParameter("TermName", detailInfo.TermName));
                parameters.Add(new SQLiteParameter("Url", detailInfo.URL));
            }
            else if (jobDetail is JMImportTermDetail)
            {
                JMImportTermDetail detailInfo = jobDetail as JMImportTermDetail;
                parameters.Add(new SQLiteParameter("Term", detailInfo.Term));
                parameters.Add(new SQLiteParameter("Action", detailInfo.Action));
            }
            else if (jobDetail is JMDiscoveryExportProfileJobDetails)
            {
                JMDiscoveryExportProfileJobDetails detailInfo = jobDetail as JMDiscoveryExportProfileJobDetails;
                parameters.Add(new SQLiteParameter("ProfileName", detailInfo.ProfileName));
                parameters.Add(new SQLiteParameter("ProfileCriteria", detailInfo.ProfileCriteria));
                parameters.Add(new SQLiteParameter("Action", detailInfo.Action));
                parameters.Add(new SQLiteParameter("Status", detailInfo.Status));
                parameters.Add(new SQLiteParameter("FinishTime", detailInfo.FinishTime));
                parameters.Add(new SQLiteParameter("Comment", detailInfo.Comment));
            }
            else if (jobDetail is JMManualApprovalJobDetails)
            {
                JMManualApprovalJobDetails detailInfo = jobDetail as JMManualApprovalJobDetails;
                parameters.Add(new SQLiteParameter("ObjectLevel", detailInfo.ObjectLevel));
                parameters.Add(new SQLiteParameter("TitleOrName", detailInfo.TitleOrName));
                parameters.Add(new SQLiteParameter("Url", detailInfo.Url));
                parameters.Add(new SQLiteParameter("ApprovalStatus", detailInfo.ApprovalStatus));
                parameters.Add(new SQLiteParameter("Action", detailInfo.Action));
                parameters.Add(new SQLiteParameter("RecordOwner", detailInfo.RecordOwner));
                parameters.Add(new SQLiteParameter("RuleCriteria", detailInfo.RuleCriteria));
            }
            else if (jobDetail is JMUniqueIDSettingJobDetails)
            {
                JMUniqueIDSettingJobDetails detailInfo = jobDetail as JMUniqueIDSettingJobDetails;
                parameters.Add(new SQLiteParameter("ObjectName", detailInfo.ObjectName));
                parameters.Add(new SQLiteParameter("SourceURL", detailInfo.SourceURL));
                parameters.Add(new SQLiteParameter("ColumnName", detailInfo.ColumnName));
                parameters.Add(new SQLiteParameter("Action", detailInfo.Action));
                parameters.Add(new SQLiteParameter("UniqueID", detailInfo.UniqueID));
                parameters.Add(new SQLiteParameter("AgentName", detailInfo.AgentName));
            }
            else if (jobDetail is JMCollectionDataJobDetails)
            {
                JMCollectionDataJobDetails detailInfo = jobDetail as JMCollectionDataJobDetails;
                parameters.Add(new SQLiteParameter("ObjectName", detailInfo.ObjectName));
                parameters.Add(new SQLiteParameter("FullPath", detailInfo.FullPath));
                parameters.Add(new SQLiteParameter("AgentName", detailInfo.AgentName));
            }
            else if (jobDetail is JMSyncSecurityContainerJobDetails)
            {
                JMSyncSecurityContainerJobDetails detailInfo = jobDetail as JMSyncSecurityContainerJobDetails;
                parameters.Add(new SQLiteParameter("Container", detailInfo.Container));
                parameters.Add(new SQLiteParameter("ObjectName", detailInfo.ObjectName));
                parameters.Add(new SQLiteParameter("FullPath", detailInfo.FullPath));
            }
            else if (jobDetail is JMEnforceRetentionJobDetail)
            {
                JMEnforceRetentionJobDetail detailInfo = jobDetail as JMEnforceRetentionJobDetail;
                parameters.Add(new SQLiteParameter("ObjectName", detailInfo.ObjectName));
                parameters.Add(new SQLiteParameter("FullPath", detailInfo.SourceURL));
                parameters.Add(new SQLiteParameter("Action", detailInfo.Action));
            }
            else if (jobDetail is JMExplorerMoveJobDetails)
            {
                JMExplorerMoveJobDetails detailInfo = jobDetail as JMExplorerMoveJobDetails;
                parameters.Add(new SQLiteParameter("ObjectName", detailInfo.ObjectName));
                parameters.Add(new SQLiteParameter("ItemType", detailInfo.ItemType));
                parameters.Add(new SQLiteParameter("FullPath", detailInfo.FullPath));
                parameters.Add(new SQLiteParameter("DestinationFullPath", detailInfo.DestinationFullPath));
            }
            else if (jobDetail is JMEXOApplySettingJobDetails)
            {
                var detailInfo = jobDetail as JMEXOApplySettingJobDetails;
                parameters.Add(new SQLiteParameter("ObjectName", detailInfo.ObjectName));
                parameters.Add(new SQLiteParameter("ItemType", detailInfo.ItemType));
                parameters.Add(new SQLiteParameter("FullPath", detailInfo.FullPath));
                parameters.Add(new SQLiteParameter("Action", detailInfo.Action));
                parameters.Add(new SQLiteParameter("Classification", detailInfo.Classification));
            }
            else if (jobDetail is JMEXODataSyncJobDetails)
            {
                var detailInfo = jobDetail as JMEXODataSyncJobDetails;
                parameters.Add(new SQLiteParameter("ObjectName", detailInfo.ObjectName));
                parameters.Add(new SQLiteParameter("ItemType", detailInfo.ItemType));
                parameters.Add(new SQLiteParameter("FullPath", detailInfo.FullPath));
            }
            else if (jobDetail is JMPhysicalDisposalJobDetails)
            {
                var detailInfo = jobDetail as JMPhysicalDisposalJobDetails;
                parameters.Add(new SQLiteParameter("ObjectName", detailInfo.ObjectName));
                parameters.Add(new SQLiteParameter("ItemType", detailInfo.ItemType));
                parameters.Add(new SQLiteParameter("FullPath", detailInfo.FullPath));
                parameters.Add(new SQLiteParameter("RuleName", detailInfo.RuleName));
                parameters.Add(new SQLiteParameter("ActionType", detailInfo.ActionType));
                parameters.Add(new SQLiteParameter("DestinationPath", detailInfo.DestinationPath));
            }
            else if (jobDetail is JMPhysicalExplorerTimerJobDetails)
            {
                var detailInfo = jobDetail as JMPhysicalExplorerTimerJobDetails;
                parameters.Add(new SQLiteParameter("ObjectName", detailInfo.ObjectName));
                parameters.Add(new SQLiteParameter("ItemType", detailInfo.ItemType));
                parameters.Add(new SQLiteParameter("FullPath", detailInfo.FullPath));
                parameters.Add(new SQLiteParameter("RuleName", detailInfo.RuleName));
            }
            else if (jobDetail is JMConnectorTimerJobDetails)
            {
                var detailInfo = jobDetail as JMConnectorTimerJobDetails;
                parameters.Add(new SQLiteParameter("ObjectName", detailInfo.ObjectName));
                parameters.Add(new SQLiteParameter("TermName", detailInfo.TermName));
                parameters.Add(new SQLiteParameter("FullPath", detailInfo.ConnectorName));
                parameters.Add(new SQLiteParameter("RuleName", detailInfo.RuleName));
            }
            else if (jobDetail is JMExportBarcodeJobDetail)
            {
                var detailInfo = jobDetail as JMExportBarcodeJobDetail;
                parameters.Add(new SQLiteParameter("ObjectName", detailInfo.ObjectName));
                parameters.Add(new SQLiteParameter("ItemType", detailInfo.ItemType));
                parameters.Add(new SQLiteParameter("FullPath", detailInfo.FullPath));
            }
            else if (jobDetail is JMAzureFileShareDataSyncDetail)
            {
                var detailInfo = jobDetail as JMAzureFileShareDataSyncDetail;
                parameters.Add(new SQLiteParameter("ObjectName", detailInfo.ObjectName));
                parameters.Add(new SQLiteParameter("FullPath", detailInfo.FullPath));
                parameters.Add(new SQLiteParameter("NodeType", detailInfo.ItemType));
            }
            else if (jobDetail is JMBoxDataSyncDetail)
            {
                var detailInfo = jobDetail as JMBoxDataSyncDetail;
                parameters.Add(new SQLiteParameter("ObjectName", detailInfo.ObjectName));
                parameters.Add(new SQLiteParameter("FullPath", detailInfo.FullPath));
                parameters.Add(new SQLiteParameter("NodeType", detailInfo.ItemType));
            }
            else if (jobDetail is JMSetPermissionJobDetails)
            {
                var detailInfo = jobDetail as JMSetPermissionJobDetails;
                parameters.Add(new SQLiteParameter("ObjectName", detailInfo.ObjectName));
                parameters.Add(new SQLiteParameter("ItemType", detailInfo.ItemType));
                parameters.Add(new SQLiteParameter("FullPath", detailInfo.FullPath));
            }
            else if (jobDetail is JMImportSPSettingDetail)
            {
                var detailInfo = jobDetail as JMImportSPSettingDetail;
                parameters.Add(new SQLiteParameter("ObjectName", detailInfo.ObjectName));
                parameters.Add(new SQLiteParameter("Url", detailInfo.Url));
            }
            else if (jobDetail is JMActionOnlyJobDetails)
            {
                var detailInfo = jobDetail as JMActionOnlyJobDetails;
                parameters.Add(new SQLiteParameter("ObjectName", detailInfo.ObjectName));
                parameters.Add(new SQLiteParameter("Url", detailInfo.Url));
                parameters.Add(new SQLiteParameter("RuleName", detailInfo.RuleName));
            }
            else if (jobDetail is JMFSDashBoardJobDetail)
            {
                var detailInfo = jobDetail as JMFSDashBoardJobDetail;
                parameters.Add(new SQLiteParameter("Action", detailInfo.Action));
            }
            else if (jobDetail is JMDashboardJobDetail)
            {
                var detailInfo = jobDetail as JMDashboardJobDetail;
                parameters.Add(new SQLiteParameter("Action", detailInfo.Action));
                parameters.Add(new SQLiteParameter("SourceFlag", detailInfo.SourceFlag));
            }
            else if (jobDetail is JMSPOnPremDashBoardJobDetail)
            {
                var detailInfo = jobDetail as JMSPOnPremDashBoardJobDetail;
                parameters.Add(new SQLiteParameter("Action", detailInfo.Action));
            }
            else if (jobDetail is FSDataSyncJobReportDetail)
            {
                var detailInfo = jobDetail as FSDataSyncJobReportDetail;
                parameters.Add(new SQLiteParameter("ObjectName", detailInfo.ObjectName));
                parameters.Add(new SQLiteParameter("FullPath", detailInfo.FullPath));
                parameters.Add(new SQLiteParameter("AgentName", detailInfo.AgentName));
                if (jobDetail is FSDataSyncJobReportDetailV2 detailV2)
                {
                    parameters.Add(new SQLiteParameter("Depth", detailV2.Depth));
                    parameters.Add(new SQLiteParameter("DirPath", detailV2.DirPath));
                }
            }
            else if (jobDetail is JMSyncRemoteNodesJobDetails)
            {
                var detailInfo = jobDetail as JMSyncRemoteNodesJobDetails;
                parameters.Add(new SQLiteParameter("Container", detailInfo.Container));
                parameters.Add(new SQLiteParameter("ObjectName", detailInfo.ObjectName));
                parameters.Add(new SQLiteParameter("ItemType", detailInfo.ItemType));
                parameters.Add(new SQLiteParameter("Action", detailInfo.Action));
            }
            else if (jobDetail is JMScanLocalNodesJobDetails)
            {
                var detailInfo = jobDetail as JMScanLocalNodesJobDetails;
                parameters.Add(new SQLiteParameter("ObjectName", detailInfo.ObjectName));
                parameters.Add(new SQLiteParameter("FullPath", detailInfo.FullPath));
                parameters.Add(new SQLiteParameter("ItemType", detailInfo.ItemType));
                parameters.Add(new SQLiteParameter("Action", detailInfo.Action));
                parameters.Add(new SQLiteParameter("AgentName", detailInfo.AgentName));
            }
            else if (jobDetail is JMFSDisposalJobDetails)
            {
                var detailInfo = jobDetail as JMFSDisposalJobDetails;
                //parameters.Add(new SQLiteParameter("DetailTab", detailInfo.DetailTab));
                parameters.Add(new SQLiteParameter("Type", detailInfo.Type));
                parameters.Add(new SQLiteParameter("ObjectName", detailInfo.ObjectName));
                parameters.Add(new SQLiteParameter("Size", detailInfo.Size));
                parameters.Add(new SQLiteParameter("SourceLocation", detailInfo.SourceLocation));
                parameters.Add(new SQLiteParameter("DestinationLocation", detailInfo.DestinationLocation));
                parameters.Add(new SQLiteParameter("FinishTime", detailInfo.FinishTime));
                parameters.Add(new SQLiteParameter("RuleName", detailInfo.RuleName));
                parameters.Add(new SQLiteParameter("Action", detailInfo.Action));
                parameters.Add(new SQLiteParameter("AgentName", detailInfo.AgentName));
                if (jobDetail is JMFSDisposalJobDetailV2 detailV2)
                {
                    parameters.Add(new SQLiteParameter("Depth", detailV2.Depth));
                    parameters.Add(new SQLiteParameter("DirPath", detailV2.DirPath));
                    parameters.Add(new SQLiteParameter("DetailAction", detailV2.DetailAction));
                }
            }
            else if (jobDetail is JMFSReclassifierJobDetails)
            {
                var detailInfo = jobDetail as JMFSReclassifierJobDetails;
                parameters.Add(new SQLiteParameter("ObjectName", detailInfo.ObjectName));
                parameters.Add(new SQLiteParameter("FullPath", detailInfo.FullPath));
                parameters.Add(new SQLiteParameter("FinishTime", detailInfo.FinishTime));
                parameters.Add(new SQLiteParameter("ItemType", detailInfo.ItemType));
            }
            else if (jobDetail is JMFSHoldJobDetails)
            {
                var detailInfo = jobDetail as JMFSHoldJobDetails;
                parameters.Add(new SQLiteParameter("ObjectName", detailInfo.ObjectName));
                parameters.Add(new SQLiteParameter("FullPath", detailInfo.FullPath));
                parameters.Add(new SQLiteParameter("Action", detailInfo.Action));
            }
            else if (jobDetail is JMGlobalSearchActionJobDetails)
            {
                var detailInfo = jobDetail as JMGlobalSearchActionJobDetails;
                //parameters.Add(new SQLiteParameter("DetailTab", detailInfo.DetailTab));
                parameters.Add(new SQLiteParameter("Type", detailInfo.Type));
                parameters.Add(new SQLiteParameter("ObjectName", detailInfo.ObjectName));
                parameters.Add(new SQLiteParameter("FullPath", detailInfo.FullPath));
                parameters.Add(new SQLiteParameter("DestinationLocation", detailInfo.DestinationLocation));
                parameters.Add(new SQLiteParameter("Action", detailInfo.Action));
            }
            else if (jobDetail is JMOnPremiseSPEnforceRuleActionJobDetails)
            {
                var detailInfo = jobDetail as JMOnPremiseSPEnforceRuleActionJobDetails;
                //parameters.Add(new SQLiteParameter("DetailTab", detailInfo.DetailTab));
                parameters.Add(new SQLiteParameter("Type", detailInfo.Type));
                parameters.Add(new SQLiteParameter("ObjectName", detailInfo.ObjectName));
                parameters.Add(new SQLiteParameter("Size", detailInfo.Size));
                parameters.Add(new SQLiteParameter("SourceLocation", detailInfo.SourceLocation));
                //parameters.Add(new SQLiteParameter("DestinationLocation", detailInfo.DestinationLocation));
                parameters.Add(new SQLiteParameter("FinishTime", detailInfo.FinishTime));
                parameters.Add(new SQLiteParameter("RuleName", detailInfo.RuleName));
                parameters.Add(new SQLiteParameter("Action", detailInfo.Action));
                parameters.Add(new SQLiteParameter("AgentName", detailInfo.AgentName));
            }
            else if (jobDetail is JMExportSearchResultJobDetails)
            {
                var detailInfo = jobDetail as JMExportSearchResultJobDetails;
                parameters.Add(new SQLiteParameter("ExportLocation", detailInfo.ExportLocation));
                parameters.Add(new SQLiteParameter("ReportName", detailInfo.ReportName));
            }
            else if (jobDetail is JMPhyBoxLoanJobDetails)
            {
                var detailInfo = jobDetail as JMPhyBoxLoanJobDetails;
                parameters.Add(new SQLiteParameter("Name", detailInfo.Name));
                parameters.Add(new SQLiteParameter("Level", detailInfo.Level));
            }
            else if (jobDetail is JMTenantUpgradeDetails)
            {
                var detailInfo = jobDetail as JMTenantUpgradeDetails;
                parameters.Add(new SQLiteParameter("UpgradeModule", detailInfo.UpgradeModule));
            }
            else if (jobDetail is JMManualApprovalSettingScheduleDetail)
            {
                var detailInfo = jobDetail as JMManualApprovalSettingScheduleDetail;
                parameters.Add(new SQLiteParameter("TitleOrName", detailInfo.TitleOrName));
                parameters.Add(new SQLiteParameter("Action", detailInfo.Action));
            }
            else if (jobDetail is JMClientAuditReportJobDetails)
            {
                var detailInfo = jobDetail as JMClientAuditReportJobDetails;
                parameters.Add(new SQLiteParameter("Type", detailInfo.Type));
                parameters.Add(new SQLiteParameter("ObjectPath", detailInfo.ObjectPath));
                parameters.Add(new SQLiteParameter("Count", detailInfo.Count));
            }
            else if (jobDetail is JMEXOEnforceRuleActionJobDetails)
            {
                var detailInfo = jobDetail as JMEXOEnforceRuleActionJobDetails;
                parameters.Add(new SQLiteParameter("Action", detailInfo.Action));
                parameters.Add(new SQLiteParameter("ObjectName", detailInfo.ObjectName));
                parameters.Add(new SQLiteParameter("ItemType", detailInfo.ItemType));
                parameters.Add(new SQLiteParameter("FullPath", detailInfo.FullPath));
                parameters.Add(new SQLiteParameter("RuleName", detailInfo.RuleName));
                parameters.Add(new SQLiteParameter("DestinationUrl", detailInfo.DestinationUrl));
            }
            else if (jobDetail is JMPickCompleteJobDetails)
            {
                var detailInfo = jobDetail as JMPickCompleteJobDetails;
                parameters.Add(new SQLiteParameter("Name", detailInfo.Name));
                parameters.Add(new SQLiteParameter("FullPath", detailInfo.FullPath));
            }
            else if (jobDetail is JMTrainingJobDetails)
            {
                var detailInfo = jobDetail as JMTrainingJobDetails;
                parameters.Add(new SQLiteParameter("TermName", detailInfo.TermName));
                parameters.Add(new SQLiteParameter("Name", detailInfo.FileName));
                parameters.Add(new SQLiteParameter("FullPath", detailInfo.FullPath));
            }
            else if (jobDetail is JMArchiverActionJobDetails)
            {
                var detailInfo = jobDetail as JMArchiverActionJobDetails;
                parameters.Add(new SQLiteParameter("ActionTab", detailInfo.ActionTab));
                parameters.Add(new SQLiteParameter("Level", detailInfo.Level));
                parameters.Add(new SQLiteParameter("SourceLocation", detailInfo.SourceLocation));
                parameters.Add(new SQLiteParameter("DestinationLocation", detailInfo.DestinationLocation));
                parameters.Add(new SQLiteParameter("Action", detailInfo.Action));
                parameters.Add(new SQLiteParameter("RuleName", detailInfo.RuleName));
                parameters.Add(new SQLiteParameter("Size", detailInfo.Size));
                parameters.Add(new SQLiteParameter("FinishTime", detailInfo.FinishTime));
                parameters.Add(new SQLiteParameter("CreatedDate", detailInfo.Created));
                parameters.Add(new SQLiteParameter("CreatedBy", detailInfo.CreatedBy));
                parameters.Add(new SQLiteParameter("ModifiedDate", detailInfo.Modified));
                parameters.Add(new SQLiteParameter("ModifiedBy", detailInfo.ModifiedBy));
                parameters.Add(new SQLiteParameter("RuleMatchFile", detailInfo.RuleMatchFile));
            }
            else if (jobDetail is JMGDriveRestoreActionJobDetail)
            {
                var detailInfo = jobDetail as JMGDriveRestoreActionJobDetail;
                parameters.Add(new SQLiteParameter("DriveId", detailInfo.DriveId));
                parameters.Add(new SQLiteParameter("Level", detailInfo.Level));
                parameters.Add(new SQLiteParameter("SourceLocation", detailInfo.SourceLocation));
                parameters.Add(new SQLiteParameter("Size", detailInfo.Size));
                parameters.Add(new SQLiteParameter("FinishTime", detailInfo.FinishTime));
                parameters.Add(new SQLiteParameter("ConflictResolution", detailInfo.ConflictResolution.ToString()));
                parameters.Add(new SQLiteParameter("Path", detailInfo.Path));
            }
            else if (jobDetail is JMRestoreActionJobDetailes)
            {
                var detailInfo = jobDetail as JMRestoreActionJobDetailes;
                parameters.Add(new SQLiteParameter("Level", detailInfo.Level));
                parameters.Add(new SQLiteParameter("SourceLocation", detailInfo.SourceLocation));
                parameters.Add(new SQLiteParameter("Size", detailInfo.Size));
                parameters.Add(new SQLiteParameter("FinishTime", detailInfo.FinishTime));
                parameters.Add(new SQLiteParameter("ConflictResolution", detailInfo.ConflictResolution.ToString()));
                parameters.Add(new SQLiteParameter("Path", detailInfo.Path));
                parameters.Add(new SQLiteParameter("PathMd5", detailInfo.PathMd5));
                parameters.Add(new SQLiteParameter("PolicyLevel", detailInfo.PolicyLevel));
                parameters.Add(new SQLiteParameter("DestinationUrl", detailInfo.DestinationUrl ?? string.Empty));
                if (jobDetail is JMMigrationRestoreActionJobDetailes migrationDetail)
                {
                    parameters.Add(new SQLiteParameter("StartTime", migrationDetail.StartTime));
                }
            }
            else if (jobDetail is JMArchiverMoveIndexJobDetails)
            {
                var detailInfo = jobDetail as JMArchiverMoveIndexJobDetails;
                parameters.Add(new SQLiteParameter("SiteCollectionURL", detailInfo.SiteUrl));
                parameters.Add(new SQLiteParameter("SourceLocation", detailInfo.SrcStorageName));
                parameters.Add(new SQLiteParameter("DestinationLocation", detailInfo.DesStorageName));
                parameters.Add(new SQLiteParameter("Size", detailInfo.Size));
            }
            else if (jobDetail is JMVEOMergeJobDetails)
            {
                var detailInfo = jobDetail as JMVEOMergeJobDetails;
                parameters.Add(new SQLiteParameter("FileName", detailInfo.FileName));
                parameters.Add(new SQLiteParameter("SourceLocation", detailInfo.SourceLocation));
                parameters.Add(new SQLiteParameter("Size", detailInfo.Size));
                parameters.Add(new SQLiteParameter("DestinationLocation", detailInfo.DestinationLocation));
                parameters.Add(new SQLiteParameter("FinishTime", detailInfo.FinishTime));
            }
            else if (jobDetail is JMArchiverRententionDashboardDetails)
            {
                var detailInfo = jobDetail as JMArchiverRententionDashboardDetails;
                parameters.Add(new SQLiteParameter("SiteCollectionURL", detailInfo.SiteUrl));
                parameters.Add(new SQLiteParameter("JobId", detailInfo.JobId));
                parameters.Add(new SQLiteParameter("SourceLocation", detailInfo.SrcStorageName));
                parameters.Add(new SQLiteParameter("DestinationLocation", detailInfo.DesStorageName));
                parameters.Add(new SQLiteParameter("Size", detailInfo.Size));
                parameters.Add(new SQLiteParameter("Action", detailInfo.Action));
                parameters.Add(new SQLiteParameter("FileName", detailInfo.FileName));
                parameters.Add(new SQLiteParameter("SourceFlag", detailInfo.SourceFlag));
                parameters.Add(new SQLiteParameter("RetentionSource", detailInfo.RetentionSource));
                parameters.Add(new SQLiteParameter("RetentionKeepDate", detailInfo.RetentionKeepDate));
                parameters.Add(new SQLiteParameter("RetentionKeepDateUnit", detailInfo.RetentionKeepDateUnit));
            }
            else if (jobDetail is JMArchiverRententionJobDetails)
            {
                var detailInfo = jobDetail as JMArchiverRententionJobDetails;
                parameters.Add(new SQLiteParameter("SiteCollectionURL", detailInfo.SiteUrl));
                parameters.Add(new SQLiteParameter("JobId", detailInfo.JobId));
                parameters.Add(new SQLiteParameter("SourceLocation", detailInfo.SrcStorageName));
                parameters.Add(new SQLiteParameter("DestinationLocation", detailInfo.DesStorageName));
                parameters.Add(new SQLiteParameter("Size", detailInfo.Size));
                parameters.Add(new SQLiteParameter("Action", detailInfo.Action));
            }
            else if (jobDetail is JMDeleteOrphanDatasJobDetails)
            {
                var detailInfo = jobDetail as JMDeleteOrphanDatasJobDetails;
                parameters.Add(new SQLiteParameter("SiteCollectionURL", detailInfo.SiteUrl));
                parameters.Add(new SQLiteParameter("JobId", detailInfo.JobId));
                parameters.Add(new SQLiteParameter("Size", detailInfo.Size));
            }
            else if (jobDetail is JMArchiverDedupJobDetails)
            {
                var detailInfo = jobDetail as JMArchiverDedupJobDetails;
                parameters.Add(new SQLiteParameter("Date", detailInfo.DedupTime));
                parameters.Add(new SQLiteParameter("Size", detailInfo.Size));
                parameters.Add(new SQLiteParameter("SrcURL", detailInfo.SrcURL));
                parameters.Add(new SQLiteParameter("SubJobId", detailInfo.SubJobId));
                parameters.Add(new SQLiteParameter("Remark9", detailInfo.Name));
                parameters.Add(new SQLiteParameter("Remark10", detailInfo.ModifyTime));
                parameters.Add(new SQLiteParameter("Remark11", detailInfo.BackupSubJobId));
                parameters.Add(new SQLiteParameter("Remark12", detailInfo.NewFileStoragePath));
                parameters.Add(new SQLiteParameter("Remark13", detailInfo.OldFileStoragePath));
            }
            else if(jobDetail is JMArchiverDedupReportDetails)
            {
                var detailInfo = jobDetail as JMArchiverDedupReportDetails;
                parameters.Add(new SQLiteParameter("Date", detailInfo.Date));
                parameters.Add(new SQLiteParameter("Size", detailInfo.Size));
                parameters.Add(new SQLiteParameter("SrcURL", detailInfo.SrcURL));
                parameters.Add(new SQLiteParameter("SubJobId", detailInfo.SubJobId));
                parameters.Add(new SQLiteParameter("Remark1", detailInfo.Remark1));
            }
            else if (jobDetail is JMArchiverRebuildStubJobDetails)
            {
                var detailInfo = jobDetail as JMArchiverRebuildStubJobDetails;
                parameters.Add(new SQLiteParameter("SiteCollectionURL", detailInfo.SiteUrl));
                parameters.Add(new SQLiteParameter("JobId", detailInfo.JobId));
                parameters.Add(new SQLiteParameter("SourceLocation", detailInfo.StubUrl));
                parameters.Add(new SQLiteParameter("DestinationLocation", detailInfo.StubUrl));
                parameters.Add(new SQLiteParameter("Size", 0));
            }
            else if (jobDetail is JMArchiverRebuildIndexJobDetails)
            {
                var detailInfo = jobDetail as JMArchiverRebuildIndexJobDetails;
                parameters.Add(new SQLiteParameter("SiteUrl", detailInfo.SiteUrl));
                parameters.Add(new SQLiteParameter("ObjectUrl", detailInfo.ObjectUrl));
                parameters.Add(new SQLiteParameter("ObjectType", detailInfo.ObjectType));
                parameters.Add(new SQLiteParameter("JobId", detailInfo.JobId));
            }
            else if (jobDetail is JMSOSummaryDetails)
            {
                var detailInfo = jobDetail as JMSOSummaryDetails;
                parameters.Add(new SQLiteParameter("Statistics", SerializerHelper.SerializeByJsonSerializer(detailInfo)));
            }
            else if (jobDetail is JMRestoreSummaryDetails)
            {
                var detailInfo = jobDetail as JMRestoreSummaryDetails;
                parameters.Add(new SQLiteParameter("Statistics", SerializerHelper.SerializeByJsonSerializer(detailInfo)));
            }
            else if (jobDetail is JMArchiverDedupReportSummaryDetails)
            {
                var detailInfo = jobDetail as JMArchiverDedupReportSummaryDetails;
                parameters.Add(new SQLiteParameter("Statistics", SerializerHelper.SerializeByJsonSerializer(detailInfo)));
            }
            else if (jobDetail is JMArchiverMigrationJobDetails)
            {
                var detailInfo = jobDetail as JMArchiverMigrationJobDetails;
                parameters.Add(new SQLiteParameter("ObjectName", detailInfo.ObjectName));
                parameters.Add(new SQLiteParameter("ObjectType", detailInfo.ObjectType));
            }
            else if (jobDetail is JMSOJobSizeStatistics)
            {
                var detailInfo = jobDetail as JMSOJobSizeStatistics;
                parameters.Add(new SQLiteParameter("Action", detailInfo.Action));
                parameters.Add(new SQLiteParameter("SourceLocation", detailInfo.SourceLocation));
                parameters.Add(new SQLiteParameter("Size", detailInfo.Size));
                parameters.Add(new SQLiteParameter("FinishTime", detailInfo.FinishTime));
                parameters.Add(new SQLiteParameter("KeepDataOption", detailInfo.KeepDataOption));
                parameters.Add(new SQLiteParameter("AuthorID", detailInfo.AuthorID));
                parameters.Add(new SQLiteParameter("AuthorEmail", detailInfo.AuthorEmail));
                parameters.Add(new SQLiteParameter("ModifiedID", detailInfo.ModifiedID));
                parameters.Add(new SQLiteParameter("ModifiedEmail", detailInfo.ModifiedEmail));
                parameters.Add(new SQLiteParameter("CreateTime", detailInfo.CreateTime));
                parameters.Add(new SQLiteParameter("ModifiedTime", detailInfo.ModifiedTime));
                parameters.Add(new SQLiteParameter("VersionCount", detailInfo.VersionCount));
            }
            else if (jobDetail is JMArchiverFullTextIndexJobDetails)
            {
                var detailInfo = jobDetail as JMArchiverFullTextIndexJobDetails;
                parameters.Add(new SQLiteParameter("Url", detailInfo.Url));
            }
            else if (jobDetail is JMArchiverDeleteRestoredDataJobDetails)
            {
                var detailInfo = jobDetail as JMArchiverDeleteRestoredDataJobDetails;
                parameters.Add(new SQLiteParameter("Url", detailInfo.Url));
                parameters.Add(new SQLiteParameter("RestoredUrl", detailInfo.RestoredUrl));
                parameters.Add(new SQLiteParameter("CleanOption", detailInfo.CleanOption));
                parameters.Add(new SQLiteParameter("CleanDelayDays", detailInfo.CleanDelayDays));
                parameters.Add(new SQLiteParameter("IsRelatedDelete", detailInfo.IsRelatedDelete));
            }
            else if (jobDetail is JMDiscoveryJobV2Details)
            {
                var detailInfo = jobDetail as JMDiscoveryJobV2Details;
                parameters.Add(new SQLiteParameter("Url", detailInfo.Url));
            }
            else if (jobDetail is JMDownloadJobReport)
            {
                var detailInfo = jobDetail as JMDownloadJobReport;
                parameters.Add(new SQLiteParameter("JobId", detailInfo.JobId));
                parameters.Add(new SQLiteParameter("Status", detailInfo.Status));
                parameters.Add(new SQLiteParameter("Comment", detailInfo.Comment));
            }
            else if (jobDetail is JMRestoreScDetails)
            {
                var detailInfo = jobDetail as JMRestoreScDetails;
                parameters.Add(new SQLiteParameter("Level", detailInfo.Level));
                parameters.Add(new SQLiteParameter("Name", detailInfo.Name));
                parameters.Add(new SQLiteParameter("SourceURL", detailInfo.SourceURL));
                parameters.Add(new SQLiteParameter("Size", detailInfo.Size));
                parameters.Add(new SQLiteParameter("JobId", detailInfo.JobId));
                parameters.Add(new SQLiteParameter("StartTime", detailInfo.StartTime));
                parameters.Add(new SQLiteParameter("FinishTime", detailInfo.FinishTime));
                parameters.Add(new SQLiteParameter("RestoreBy", detailInfo.RestoreBy));
                parameters.Add(new SQLiteParameter("RestoreTo", detailInfo.RestoreTo));
                parameters.Add(new SQLiteParameter("IsDaoMigration", detailInfo.IsDaoMigration));
                parameters.Add(new SQLiteParameter("IsEndUserOpt", detailInfo.IsEndUserOpt));
            }
            else if (jobDetail is JMRestoreGDriveDetails)
            {
                var detailInfo = jobDetail as JMRestoreGDriveDetails;
                parameters.Add(new SQLiteParameter("Level", detailInfo.Level));
                parameters.Add(new SQLiteParameter("Name", detailInfo.Name));
                parameters.Add(new SQLiteParameter("SourceURL", detailInfo.SourceURL));
                parameters.Add(new SQLiteParameter("Size", detailInfo.Size));
                parameters.Add(new SQLiteParameter("JobId", detailInfo.JobId));
                parameters.Add(new SQLiteParameter("StartTime", detailInfo.StartTime));
                parameters.Add(new SQLiteParameter("FinishTime", detailInfo.FinishTime));
                parameters.Add(new SQLiteParameter("RestoreBy", detailInfo.RestoreBy));
                parameters.Add(new SQLiteParameter("RestoreTo", detailInfo.RestoreTo));
                parameters.Add(new SQLiteParameter("IsDaoMigration", detailInfo.IsDaoMigration));
                parameters.Add(new SQLiteParameter("IsEndUserOpt", detailInfo.IsEndUserOpt));
            }
            else if (jobDetail is JMRestoreReportJobDetailes)
            {
                var detailInfo = jobDetail as JMRestoreReportJobDetailes;
                parameters.Add(new SQLiteParameter("Level", detailInfo.Level));
                parameters.Add(new SQLiteParameter("Title", detailInfo.Title));
                parameters.Add(new SQLiteParameter("Url", detailInfo.Url));
            }
            else if (jobDetail is JMGoogleJobDetails)
            {
                var detailInfo = jobDetail as JMGoogleJobDetails;
                parameters.Add(new SQLiteParameter("ObjectName", detailInfo.ObjectName));
                parameters.Add(new SQLiteParameter("FullPath", detailInfo.FullPath));
                parameters.Add(new SQLiteParameter("NodeType", detailInfo.ItemType));
                parameters.Add(new SQLiteParameter("Comment", detailInfo.Comment));
                parameters.Add(new SQLiteParameter("FileSize", detailInfo.FileSize));
                parameters.Add(new SQLiteParameter("Status", detailInfo.Status));
                parameters.Add(new SQLiteParameter("Classification", detailInfo.Classification));
                parameters.Add(new SQLiteParameter("Action", detailInfo.Action));
            }
            else if (jobDetail is JMGoogleLabelJobDetails)
            {
                var detailInfo = jobDetail as JMGoogleLabelJobDetails;
                parameters.Add(new SQLiteParameter("LabelId", detailInfo.LabelId));
                parameters.Add(new SQLiteParameter("LabelName", detailInfo.LabelName));
                parameters.Add(new SQLiteParameter("Action", detailInfo.Action));
                parameters.Add(new SQLiteParameter("TenantId", detailInfo.TenantId));
            }
            else if (jobDetail is JMGoogleDataSyncJobDetails)
            {
                var detailInfo = jobDetail as JMGoogleDataSyncJobDetails;
                parameters.Add(new SQLiteParameter("ObjectName", detailInfo.ObjectName));
                parameters.Add(new SQLiteParameter("FullPath", detailInfo.FullPath));
                parameters.Add(new SQLiteParameter("Status", detailInfo.Status));
                parameters.Add(new SQLiteParameter("Comment", detailInfo.Comment));
                parameters.Add(new SQLiteParameter("ItemType", detailInfo.ItemType));
               
            }
            else if (jobDetail is JMSalesforceDiscoveryJob)
            {
                var detailInfo = jobDetail as JMSalesforceDiscoveryJob;
                parameters.Add(new SQLiteParameter("ObjectName", detailInfo.ObjectName));
                parameters.Add(new SQLiteParameter("ObjectType", detailInfo.ObjectType));
                parameters.Add(new SQLiteParameter("TotalItemCount", detailInfo.TotalItemCount));
                parameters.Add(new SQLiteParameter("TotalSize", detailInfo.TotalSize));
                parameters.Add(new SQLiteParameter("TenantId", detailInfo.TenantId));
               
            }
            else if (jobDetail is JMPhysicalTemplateImportJobDetail)
            {
                JMPhysicalTemplateImportJobDetail detailInfo = jobDetail as JMPhysicalTemplateImportJobDetail;
                parameters.Add(new SQLiteParameter("TemplateSuiteName", detailInfo.TemplateSuiteName));
                parameters.Add(new SQLiteParameter("TemplateSuiteStartFrom", detailInfo.TemplateSuiteStartFrom));
                parameters.Add(new SQLiteParameter("TemplateName", detailInfo.TemplateName));
                parameters.Add(new SQLiteParameter("TemplateType", detailInfo.TemplateType));
                parameters.Add(new SQLiteParameter("TemplatePrefix", detailInfo.TemplatePrefix));
                parameters.Add(new SQLiteParameter("TemplateDigits", detailInfo.TemplateDigits));
            }
            else if (jobDetail is JMHoldRecordsImportJobDetail  )
            {
                JMHoldRecordsImportJobDetail detailInfo = jobDetail as JMHoldRecordsImportJobDetail;
                parameters.Add(new SQLiteParameter("Name", detailInfo.Name));
                parameters.Add(new SQLiteParameter("Url", detailInfo.Url));
                parameters.Add(new SQLiteParameter("HoldTitle", detailInfo.HoldTitle));
            }
            else if (jobDetail is JMWorkspaceHoldImportJobDetail)
            {
                JMWorkspaceHoldImportJobDetail detailInfo = jobDetail as JMWorkspaceHoldImportJobDetail;
                parameters.Add(new SQLiteParameter("Url", detailInfo.Url));
                parameters.Add(new SQLiteParameter("Type", detailInfo.Type));
                parameters.Add(new SQLiteParameter("HoldTitle", detailInfo.HoldTitle));
            }
            else if (jobDetail is JMConvertStubJobDetails)
            {
                JMConvertStubJobDetails detailInfo = jobDetail as JMConvertStubJobDetails;
                parameters.Add(new SQLiteParameter("FullPath", detailInfo.FullPath));
                parameters.Add(new SQLiteParameter("FinishTime", detailInfo.FinishTime));
                parameters.Add(new SQLiteParameter("Action", detailInfo.Action));
            }
            else if(jobDetail is JMDiscoveryGoogleJobDetails)
            {
                JMDiscoveryGoogleJobDetails details = jobDetail as JMDiscoveryGoogleJobDetails;
                parameters.Add(new SQLiteParameter("DriveName", details.DriveName));
                parameters.Add(new SQLiteParameter("Status", details.Status));
                parameters.Add(new SQLiteParameter("Comment", details.Comment));
            }
            else if (jobDetail is JMDiscoveryFileSystemJobDetails)
            {
                JMDiscoveryFileSystemJobDetails details = jobDetail as JMDiscoveryFileSystemJobDetails;
                parameters.Add(new SQLiteParameter("ConnectionName", details.ConnectionName));
                parameters.Add(new SQLiteParameter("Status", details.Status));
                parameters.Add(new SQLiteParameter("Comment", details.Comment));
            }
            else if (jobDetail is JMImportFullTextIndexSClistJobDetail)
            {
                JMImportFullTextIndexSClistJobDetail detailInfo = jobDetail as JMImportFullTextIndexSClistJobDetail;
                parameters.Add(new SQLiteParameter("Url", detailInfo.Url));
                parameters.Add(new SQLiteParameter("ObjectName", null));
            }
            else if (jobDetail is JMDiscoveryGoogleProfileJobDetails)
            {
                JMDiscoveryGoogleProfileJobDetails details = jobDetail as JMDiscoveryGoogleProfileJobDetails;
                parameters.Add(new SQLiteParameter("ProfileName", details.ProfileName));
                parameters.Add(new SQLiteParameter("DriveName", details.DriveName));
            }
            else if (jobDetail is JMDeclaredRecordsMigrationJobDetails)
            {
                JMDeclaredRecordsMigrationJobDetails detailInfo = jobDetail as JMDeclaredRecordsMigrationJobDetails;
                parameters.Add(new SQLiteParameter("Url", detailInfo.Url));
                parameters.Add(new SQLiteParameter("FinishTime", detailInfo.FinishTime));
            }
            else if (jobDetail is JMStubDisposalJobDetails)
            {
                JMStubDisposalJobDetails detailInfo = jobDetail as JMStubDisposalJobDetails;
                parameters.Add(new SQLiteParameter("Url", detailInfo.Url));
                parameters.Add(new SQLiteParameter("FinishTime", detailInfo.FinishTime));
            }
            else if (jobDetail is JMDeleteArchivedSCJobDetails)
            {
                JMDeleteArchivedSCJobDetails detailInfo = jobDetail as JMDeleteArchivedSCJobDetails;
                parameters.Add(new SQLiteParameter("Url", detailInfo.Url));
                parameters.Add(new SQLiteParameter("JobId", detailInfo.JobId));
                parameters.Add(new SQLiteParameter("Size", detailInfo.Size));
                parameters.Add(new SQLiteParameter("SourceStorageName", detailInfo.SourceStorageName));
            }
            else if (jobDetail is JMArchiverJobProgressDetails)
            {
                var detailInfo = jobDetail as JMArchiverJobProgressDetails;
                // Main Job details
                parameters.Add(new SQLiteParameter("SubJobID", detailInfo.SubJobID));
                parameters.Add(new SQLiteParameter("Status", detailInfo.Status));
                parameters.Add(new SQLiteParameter("Scope", detailInfo.Scope));
                parameters.Add(new SQLiteParameter("Successful", detailInfo.SuccessfulCount));
                parameters.Add(new SQLiteParameter("Failed", detailInfo.FailedCount));
                parameters.Add(new SQLiteParameter("Skipped", detailInfo.SkippedCount));
                parameters.Add(new SQLiteParameter("Comment", detailInfo.Comment));
                parameters.Add(new SQLiteParameter("IsSavedJobDetails", detailInfo.IsSavedJobDetails));

                // Progress
                parameters.Add(new SQLiteParameter("ProgressStatus", detailInfo.ProgressStatus));
                parameters.Add(new SQLiteParameter("StartTime", detailInfo.StartTime.Ticks));
                parameters.Add(new SQLiteParameter("FinishTime", detailInfo.FinishTime.Ticks));
                parameters.Add(new SQLiteParameter("LastUpdatedTime", detailInfo.LastUpdatedTime.Ticks));

                parameters.Add(new SQLiteParameter("TotalFiles", detailInfo.TotalFiles));

                parameters.Add(new SQLiteParameter("TotalMatchedRuleFilesForExport", detailInfo.TotalMatchedRuleFilesForExport));
                parameters.Add(new SQLiteParameter("TotalMatchedRuleFilesForArchive", detailInfo.TotalMatchedRuleFilesForArchive));
                parameters.Add(new SQLiteParameter("TotalMatchedRuleFilesForOtherActions", detailInfo.TotalMatchedRuleFilesForOtherActions));

                parameters.Add(new SQLiteParameter("ProcessedItemsInfos", JsonConvert.SerializeObject(new List<ProcessedItemsInfoDto>
                {
                    detailInfo.ProcessedScannedItemsInfo,
                    detailInfo.ProcessedExportedItemsInfo,
                    detailInfo.ProcessedArchivedItemsInfo,
                    detailInfo.ProcessedOtherItemsInfo
                })));

                parameters.Add(new SQLiteParameter("StartScanTime", detailInfo.StartScanTime.Ticks));
                parameters.Add(new SQLiteParameter("EstimatedScanFinishedTime", detailInfo.EstimatedScanFinishedTime.Ticks));

                parameters.Add(new SQLiteParameter("StartExportTime", detailInfo.StartExportTime.Ticks));
                parameters.Add(new SQLiteParameter("EstimatedExportFinishedTime", detailInfo.EstimatedExportFinishedTime.Ticks));

                parameters.Add(new SQLiteParameter("StartArchivedTime", detailInfo.StartArchivedTime.Ticks));
                parameters.Add(new SQLiteParameter("EstimatedArchivedFinishedTime", detailInfo.EstimatedArchivedFinishedTime.Ticks));

                parameters.Add(new SQLiteParameter("StartOtherTime", detailInfo.StartOtherTime.Ticks));
                parameters.Add(new SQLiteParameter("EstimatedOtherFinishedTime", detailInfo.EstimatedOtherFinishedTime.Ticks));
            }
            else if(jobDetail is MainDataSyncCommonDataJobDetails)
            {
                var detailInfo = jobDetail as MainDataSyncCommonDataJobDetails;
                parameters.Add(new SQLiteParameter("DataCenterName", detailInfo.DataCenterName));
                parameters.Add(new SQLiteParameter("Action", detailInfo.Action));
                parameters.Add(new SQLiteParameter("Status", detailInfo.Status));
                parameters.Add(new SQLiteParameter("Comment", detailInfo.Comment));
            }
            else if(jobDetail is OtherDCSyncCommonDataJobDetails)
            {
                var detailInfo = jobDetail as OtherDCSyncCommonDataJobDetails;
                parameters.Add(new SQLiteParameter("ActionName", detailInfo.ActionName));
                parameters.Add(new SQLiteParameter("Type", detailInfo.Type));
                parameters.Add(new SQLiteParameter("Status", detailInfo.Status));
                parameters.Add(new SQLiteParameter("Comment", detailInfo.Comment));
            }
            else if (jobDetail is JMPhysicalMoveJobDetails)
            {
                var detailInfo = jobDetail as JMPhysicalMoveJobDetails;
                parameters.Add(new SQLiteParameter("UniqueId", detailInfo.UniqueId));
                parameters.Add(new SQLiteParameter("ObjectName", detailInfo.ObjectName));
                parameters.Add(new SQLiteParameter("ItemType", detailInfo.ItemType));
                parameters.Add(new SQLiteParameter("Status", detailInfo.Status));
                parameters.Add(new SQLiteParameter("Comment", detailInfo.Comment));
            }
            if (!(jobDetail is JMTermSelection || jobDetail is JMArchiverJobProgressDetails))
            {
                parameters.Add(new SQLiteParameter("Status", jobDetail.Status));
                parameters.Add(new SQLiteParameter("Comment", jobDetail.Comment));
            }

            return parameters;
        }



        public bool IsExistTable(string reportFilePath, string tableName)
        {
            //The preferred method is to integrate CheckFileExist with this method,
            //but there are too many methods to refer to. 
            //In order to make it easier to modify and test, only in this method is modified.
            try
            {
                if (System.IO.File.Exists(reportFilePath))
                {
                    return SQLCommond.IsExistTable(reportFilePath, tableName);
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                logger.Warn($"IsExistTable error: {e}");
                return false;
            }
        }



        private string GetDetailColumnValue(SQLiteDataReader sqlReader, string name)
        {
            var existColumn = -1 != sqlReader.GetOrdinal(name);
            if (existColumn && sqlReader[name] != null)
            {
                return sqlReader[name].ToString();
            }
            return "";
        }

        private string ConvertToFormatSize(long size)
        {
            if (size < 1024)
            {
                return string.Format("{0} {1}", size, I18NEntity.GetString("RM_FS_JobReportSizeUnitBytes"));
            }
            else if (size >= 1024 && size < 1024 * 1024)
            {
                return string.Format("{0:F} {1}", size / 1024.0, I18NEntity.GetString("RM_FS_JobReportSizeUnitKB"));
            }
            else if (size >= 1024 * 1024 && size < 1024 * 1024 * 1024)
            {
                return string.Format("{0:F} {1}", size / (1024 * 1024.0), I18NEntity.GetString("RM_FS_JobReportSizeUnitMB"));
            }
            else if (size >= 1024 * 1024 * 1024 && size < 1024L * 1024 * 1024 * 1024)
            {
                return string.Format("{0:F} {1}", size / (1024 * 1024 * 1024.0), I18NEntity.GetString("RM_FS_JobReportSizeUnitGB"));
            }
            else
            {
                return string.Format("{0:F} {1}", size / (1024L * 1024 * 1024 * 1024.0), I18NEntity.GetString("RM_FS_JobReportSizeUnitTB"));
            }
        }

        private string ConvertDetailsTimeZone(string finishTime)
        {
            try
            {
                Regex reg = new Regex(@".*\(.*?\)");
                var match = reg.Match(finishTime);
                if (match?.Success ?? false)
                {
                    finishTime = match.Value;
                }
            }
            catch (Exception e)
            {
                logger.Warn($"Convert error:{e}");
            }

            return finishTime;
        }
    }
}
