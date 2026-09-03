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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common;
using AvePoint.RA.I18N.Core;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.SharePoint.Object;
using System.Data;
using AvePoint.GCommon.Utility;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class ReportCenterDao : IReportCenterDao
    {
        private RALogger logger = RALogger.GetInstance(typeof(JobMonitorDao));
        public void SaveReportJobDatas(string reportFilePath, IEnumerable<BaseReport> baseReports, string insertDataSql)
        {
            List<List<SQLiteParameter>> parameterList = new List<List<SQLiteParameter>>();
            foreach (BaseReport report in baseReports)
            {
                parameterList.Add(ReportConvertor.BuildSaveReportParameters(report));
            }
            SQLCommond.BatchExecuteNonQueryStable(reportFilePath, insertDataSql, parameterList);
        }

        public void SaveReportJobDatas(string reportFilePath, IEnumerable<IEnumerable<ReportCell>> jobDetails, string insertDataSql)
        {
            List<List<SQLiteParameter>> parameterList = new List<List<SQLiteParameter>>();
            
            foreach (var detailcells in jobDetails)
            {
                List<SQLiteParameter> parameters = new List<SQLiteParameter>();
                foreach (var item in detailcells)
                {
                    parameters.Add(new SQLiteParameter(item.Key, item.Value));
                }
                parameterList.Add(parameters);
            }
            SQLCommond.BatchExecuteNonQueryStable(reportFilePath, insertDataSql, parameterList);
        }

        public IEnumerable<BaseReport> GetReportJobDatas(string reportFilePath, string slectDataSql, BaseJobDto jobInfo)
        {
            IEnumerable<BaseReport> result = new List<BaseReport>();
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection("Data Source=" + reportFilePath))
                {
                    conn.Open();
                    try
                    {
                        //Fortify fix: Unreleased Resource: Database
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
                                result = ConvertDomainToReportInfo(sqlReader, jobInfo);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error($"Get report data failed,report file path :{reportFilePath},select data sql :{slectDataSql},error message: {e.Message},error:{e}");
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error($"Get report failed, report file path :{reportFilePath}.error: {e}");
            }
            return result;
        }

        public IEnumerable<BaseReport> ConvertDomainToReportInfo(SQLiteDataReader sqlReader, BaseJobDto jobInfo)
        {
            List<BaseReport> result = new List<BaseReport>();

            while (sqlReader.Read())
            {
                if (jobInfo.JobType == (int)JobType.BCSTermUsageReport || jobInfo.JobType == (int)JobType.EXOTermUsageReport 
                    || jobInfo.JobType == (int)JobType.PhysicalTermUsageReport|| jobInfo.JobType == (int)JobType.FSBCSTermUsageReport 
                    || jobInfo.JobType == (int)JobType.OneDriveTermUsageReport || jobInfo.JobType == (int)JobType.BoxBCSTermUsageReport
                    || jobInfo.JobType == (int)JobType.GoogleBCSTermUsageReport || jobInfo.JobType == (int)JobType.TeamsBCSTermUsageReport
                    || jobInfo.JobType == (int)JobType.TeamsOrphanedTermUsageReport || jobInfo.JobType == (int)JobType.TeamsRetiredTermUsageReport)
                {
                    result.Add(ConvertDomainToBCSTermUsageReport(sqlReader));
                }
                else if (jobInfo.JobType == (int)JobType.ItemsFilesDueDisposal || jobInfo.JobType == (int)JobType.EXOItemsFilesDueDisposalReport 
                    || jobInfo.JobType == (int)JobType.PhysicalItemsFilesDueDisposalReport|| jobInfo.JobType == (int)JobType.FSItemsFilesDueDisposal
                    || jobInfo.JobType == (int)JobType.OneDriveItemsFilesDueDisposalReport || jobInfo.JobType == (int)JobType.SPOnPremItemsFilesDueDisposal
                    || jobInfo.JobType == (int)JobType.DisposalReport || jobInfo.JobType == (int)JobType.BoxItemsFilesDueDisposalReport
                    || jobInfo.JobType == (int)JobType.GoogleItemsFilesDueDisposalReport
                    || jobInfo.JobType == (int)JobType.TeamsItemsFilesDueDisposalReport)
                {
                    result.Add(ConvertDomainToDueDisposalReport(sqlReader));
                }
                else if (jobInfo.JobType == (int)JobType.CreateAndDestroyedFileReport 
                    || jobInfo.JobType == (int)JobType.EXOCreateAndDestroyedFileReport 
                    || jobInfo.JobType == (int)JobType.PhysicalCreateAndDestroyedFileReport 
                    || jobInfo.JobType == (int)JobType.FSCreateAndDestroyedFileReport
                    || jobInfo.JobType == (int)JobType.OneDriveCreateAndDestroyedFileReport
                    || jobInfo.JobType == (int)JobType.CreateAndDestroyedReport
                    || jobInfo.JobType == (int)JobType.BoxCreateAndDestroyedFileReport
                    || jobInfo.JobType == (int)JobType.GoogleCreateAndDestroyedFileReport
                    || jobInfo.JobType == (int)JobType.TeamsCreateAndDestroyedFileReport
                    )
                {
                    result.Add(ConvertDomainToTimeFrameReport(sqlReader));
                }
                else if (jobInfo.JobType == (int)JobType.AvailableSpaceReport)
                {
                    result.Add(ConvertDomainToAvailableSpaceReport(sqlReader));
                }
                if (jobInfo.JobType == (int)JobType.SPOActionAuditReport || jobInfo.JobType == (int)JobType.OneDriveActionAuditReport
                    || jobInfo.JobType == (int)JobType.TeamsActionAuditReport)
                {
                    result.Add(ConvertDomainToAciontAuditReport(sqlReader));
                }
                if(jobInfo.JobType == (int)JobType.GenerateRestoreReport)
                {
                    result.Add(ConvertDomainToRestoreReport(sqlReader));
                }
                if (JobTypeConstants.ArchivedSiteReportJobTypes.Contains(jobInfo.JobType))
                {
                    result.Add(ConvertDomainToArchivedSiteReport(sqlReader));
                }
            }
            return result;
        }

        public int GetCountForReport(string reportFilePath, string slectDataSql, BaseJobDto jobInfo)
        {
            int result = 0;
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection("Data Source=" + reportFilePath))
                {
                    conn.Open();
                    try
                    {
                        using (SQLiteCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = slectDataSql;
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
            catch (Exception e)
            {
                logger.Error(string.Format("Get report failed. {0}", e));
            }
            return result;
        }


        public BCSTermUsageReport ConvertDomainToBCSTermUsageReport(SQLiteDataReader sqlReader)
        {
            BCSTermUsageReport reportInfo = new BCSTermUsageReport();
            reportInfo.BCSTermId = sqlReader["BCSTermId"].ToString();
            reportInfo.BCSTermName = sqlReader["BCSTermName"].ToString();
            reportInfo.CreatedBy = sqlReader["CreatedBy"].ToString();
            reportInfo.CreatedTime = sqlReader["CreatedTime"] == null ? 0 : long.Parse(sqlReader["CreatedTime"].ToString());
            reportInfo.LastModifiedBy = sqlReader["LastModifiedBy"].ToString();
            reportInfo.SPWebTimeZoneName = sqlReader["SPWebTimeZoneID"].ToString();
            reportInfo.LastModifiedTime = sqlReader["LastModifiedTime"] == null ? 0 : long.Parse(sqlReader["LastModifiedTime"].ToString());
            reportInfo.ObjectLevel = sqlReader["ObjectLevel"] == null ? 0 : Int32.Parse(sqlReader["ObjectLevel"].ToString());
            reportInfo.TitleOrName = sqlReader["TitleOrName"].ToString();
            reportInfo.Url = sqlReader["Url"].ToString();
            try
            {
                reportInfo.TermStatus = sqlReader["TermStatus"] == null ? RMTermStatus.Avaliable : (RMTermStatus)Int32.Parse(sqlReader["TermStatus"].ToString());
                reportInfo.BCSTermFullPath = sqlReader["BCSTermFullPath"].ToString();
                reportInfo.LifecycleStatus = sqlReader["LifecycleStatus"] == null ? "" : sqlReader["LifecycleStatus"].ToString();
                reportInfo.CurrentHeldBy = sqlReader["CurrentHeldBy"] == null ? "" : sqlReader["CurrentHeldBy"].ToString();
                reportInfo.Box = sqlReader["Box"] == null ? "" : sqlReader["Box"].ToString();
                reportInfo.HomeLocation = sqlReader["HomeLocation"] == null ? "" : sqlReader["HomeLocation"].ToString();
                reportInfo.Availablity = sqlReader["Availablity"] == null ? "" : sqlReader["Availablity"].ToString();
            }
            catch (Exception e)
            {
                logger.Warn("Get Physical column value exception {0}", e.ToString());
            }

            return reportInfo;
        }

        public DueDisposalReport ConvertDomainToDueDisposalReport(SQLiteDataReader sqlReader)
        {
            DueDisposalReport reportInfo = new DueDisposalReport();
            reportInfo.BCSTermId = sqlReader["BCSTermId"].ToString();
            reportInfo.BCSTermName = sqlReader["BCSTermName"].ToString();
            reportInfo.AppliedRuleId = sqlReader["AppliedRuleId"].ToString();
            reportInfo.AppliedRuleName = sqlReader["AppliedRuleName"].ToString();
            reportInfo.DisposalClass = GetReportColumnValue(sqlReader, "DisposalClass");
            reportInfo.DisposalAction = sqlReader["DisposalAction"] == null ? (int)RMContentDisposalAction.None : (int)(RMContentDisposalAction)Int32.Parse(sqlReader["DisposalAction"].ToString());
            reportInfo.CreatedBy = sqlReader["CreatedBy"].ToString();
            reportInfo.CreatedTime = sqlReader["CreatedTime"] == null ? 0 : long.Parse(sqlReader["CreatedTime"].ToString());
            reportInfo.LastModifiedBy = sqlReader["LastModifiedBy"].ToString();
            reportInfo.LastModifiedTime = sqlReader["LastModifiedTime"] == null ? 0 : long.Parse(sqlReader["LastModifiedTime"].ToString());
            reportInfo.SPWebTimeZoneName = sqlReader["SPWebTimeZoneID"].ToString();
            reportInfo.ObjectLevel = sqlReader["ObjectLevel"] == null ? 0 : Int32.Parse(sqlReader["ObjectLevel"].ToString());
            reportInfo.TitleOrName = sqlReader["TitleOrName"].ToString();
            reportInfo.SiteCollectionTitle = GetReportColumnValue(sqlReader, "SiteCollectionTitle");
            reportInfo.Url = sqlReader["Url"].ToString();
            reportInfo.ManualApproval = sqlReader["ManualApproval"] == null ? RMDisposalManualApproval.Nonsupport : (RMDisposalManualApproval)Int32.Parse(sqlReader["ManualApproval"].ToString());
            reportInfo.ExportType = sqlReader["ExportType"] == null ? RMExportTypeValue.None : (RMExportTypeValue)Int32.Parse(sqlReader["ExportType"].ToString());
            reportInfo.Status = sqlReader["Status"] == null ? RMReportStatus.Failed : (RMReportStatus)int.Parse(sqlReader["Status"].ToString());
            string comment = sqlReader["Comment"].ToString();
            try
            {
                reportInfo.RelatedRecords = sqlReader["RelatedRecords"] == null ? "" : sqlReader["RelatedRecords"].ToString();
                reportInfo.RelatedRecordsAction = sqlReader["RelatedRecordsAction"] == null ? 0 : int.Parse(sqlReader["RelatedRecordsAction"].ToString());
            }
            catch (Exception)
            {
                logger.Warn("old rpt file not contains RelatedRecords and RelatedRecordsAction");
            }
            if (!string.IsNullOrEmpty(comment))
            {
                reportInfo.Comment = I18NEntity.GetString(comment);
            }
            try
            {
                reportInfo.LifecycleStatus = sqlReader["LifecycleStatus"] == null ? "" : sqlReader["LifecycleStatus"].ToString();
                reportInfo.CurrentHeldBy = sqlReader["CurrentHeldBy"] == null ? "" : sqlReader["CurrentHeldBy"].ToString();
                reportInfo.Box = sqlReader["Box"] == null ? "" : sqlReader["Box"].ToString();
                reportInfo.HomeLocation = sqlReader["HomeLocation"] == null ? "" : sqlReader["HomeLocation"].ToString();
                reportInfo.Availablity = sqlReader["Availablity"] == null ? "" : sqlReader["Availablity"].ToString();
            }
            catch (Exception ex)
            {
                logger.Warn("Get Physical column value exception {0}", ex.ToString());
            }
            return reportInfo;
        }

        public CreateAndDestroyedFileReport ConvertDomainToTimeFrameReport(SQLiteDataReader sqlReader)
        {
            CreateAndDestroyedFileReport reportInfo = new CreateAndDestroyedFileReport();
            reportInfo.TermName = sqlReader["BCSTermName"].ToString();
            reportInfo.OperationTime = sqlReader["OperationTime"].ToString();
            reportInfo.OperationBy = I18NEntity.GetString(sqlReader["OperationBy"].ToString());
            //string objectLevel = sqlReader["ObjectLevel"].ToString();
            //if (string.Equals(objectLevel, "Document", StringComparison.OrdinalIgnoreCase))
            //{
            //    reportInfo.LevelStr = (int)RMReportObjectLevel.Document;
            //}
            //else if (string.Equals(objectLevel, "PhysicalFile", StringComparison.OrdinalIgnoreCase))
            //{
            //    reportInfo.LevelStr = (int)RMReportObjectLevel.PhysicalFile;
            //}
            reportInfo.LevelStr = sqlReader["ObjectLevel"] == null ? 0 : Int32.Parse(sqlReader["ObjectLevel"].ToString());
            reportInfo.ObjectLevel = sqlReader["ObjectLevel"] == null ? 0 : Int32.Parse(sqlReader["ObjectLevel"].ToString());
            reportInfo.Title = sqlReader["TitleOrName"].ToString();
            reportInfo.Url = sqlReader["Url"].ToString();
            reportInfo.Operation = int.Parse(sqlReader["Operation"].ToString());
            reportInfo.DisposalClass = GetReportColumnValue(sqlReader, "DisposalClass");
            reportInfo.ApprovedBy = GetReportColumnValue(sqlReader, "ApprovedBy");
            reportInfo.ApprovedByUPN = GetReportColumnValue(sqlReader, "ApprovedByUPN");
            reportInfo.CreatedTime = string.IsNullOrEmpty(GetReportColumnValue(sqlReader, "CreatedTime")) ? 0 : Int64.Parse(GetReportColumnValue(sqlReader, "CreatedTime"));
            reportInfo.LastModifiedTime = string.IsNullOrEmpty(GetReportColumnValue(sqlReader, "LastModifiedTime")) ? 0 : Int64.Parse(GetReportColumnValue(sqlReader, "LastModifiedTime"));
            reportInfo.FileType = GetReportColumnValue(sqlReader, "FileType");
            reportInfo.RecordsId = GetReportColumnValue(sqlReader, "RecordsId");
            reportInfo.RuleName = GetReportColumnValue(sqlReader, "RuleName");
            reportInfo.ApprovalStatus = string.IsNullOrEmpty(GetReportColumnValue(sqlReader, "ApprovalStatus")) ? 0 : int.Parse(sqlReader["ApprovalStatus"].ToString());
            reportInfo.InternalApprovedStatus = string.IsNullOrEmpty(GetReportColumnValue(sqlReader, "InternalApprovedStatus")) ? 0 : int.Parse(sqlReader["InternalApprovedStatus"].ToString());
            try
            {
                reportInfo.LifecycleStatus = sqlReader["LifecycleStatus"] == null ? "" : sqlReader["LifecycleStatus"].ToString();
                reportInfo.CurrentHeldBy = sqlReader["CurrentHeldBy"] == null ? "" : sqlReader["CurrentHeldBy"].ToString();
                reportInfo.Box = sqlReader["Box"] == null ? "" : sqlReader["Box"].ToString();
                reportInfo.HomeLocation = sqlReader["HomeLocation"] == null ? "" : sqlReader["HomeLocation"].ToString();
                reportInfo.Availablity = sqlReader["Availablity"] == null ? "" : sqlReader["Availablity"].ToString();
            }
            catch (Exception ex)
            {
                logger.Warn("Get Physical column value exception {0}", ex.ToString());
            }
            return reportInfo;
        }

        public AvailableSpaceReport ConvertDomainToAvailableSpaceReport(SQLiteDataReader sqlReader)
        {
            AvailableSpaceReport reportInfo = new AvailableSpaceReport();
            reportInfo.Location = sqlReader["Location"].ToString();
            reportInfo.AvailableSpace = Math.Round(Double.Parse(sqlReader["AvailableSpace"].ToString()),2);
            reportInfo.LocationSize = Math.Round(Double.Parse(sqlReader["LocationSize"].ToString()), 2);
            reportInfo.InculdingContainerInfo = sqlReader["InculdingContainerInfo"].ToString();
            return reportInfo;
        }

        public ClientSPAuditReport ConvertDomainToAciontAuditReport(SQLiteDataReader sqlReader)
        {
            ClientSPAuditReport reportInfo = new ClientSPAuditReport();
            reportInfo.ObjectLevel = sqlReader["ObjectLevel"] == null ? 0 : Int32.Parse(sqlReader["ObjectLevel"].ToString());
            reportInfo.TitleOrName = sqlReader["TitleOrName"].ToString();
            reportInfo.Url = sqlReader["Url"].ToString();
            reportInfo.User = sqlReader["UserName"].ToString();
            reportInfo.EventTypeName = sqlReader["EventTypeName"].ToString();
            reportInfo.Occurred = sqlReader["Occurred"] == null ? 0 : long.Parse(sqlReader["Occurred"].ToString());
            reportInfo.SiteUrl = sqlReader["SiteUrl"].ToString();
            reportInfo.Event = sqlReader["Event"] == null ? 0 : Int32.Parse(sqlReader["Event"].ToString());
            reportInfo.DisplayName = sqlReader["DisplayName"].ToString();
            reportInfo.Browser = sqlReader["Browser"].ToString();
            return reportInfo;
        }
        public RestoreFileReport ConvertDomainToRestoreReport(SQLiteDataReader sqlReader)
        {
            RestoreFileReport sCRestoreDetails = new RestoreFileReport();
            sCRestoreDetails.ObjectLevel = sqlReader["ObjectLevel"] == null ? 0 : Int32.Parse(sqlReader["ObjectLevel"].ToString());
            sCRestoreDetails.TitleOrName = sqlReader["TitleOrName"].ToString();
            sCRestoreDetails.Url = sqlReader["SourceURL"].ToString();
            sCRestoreDetails.Size = sqlReader.GetInt64("Size");
            sCRestoreDetails.RestoreBy = sqlReader["RestoreBy"].ToString();
            sCRestoreDetails.JobId = sqlReader["JobId"].ToString();
            sCRestoreDetails.StartTime = sqlReader.GetInt64("StartTime");
            sCRestoreDetails.EndTime = sqlReader.GetInt64("FinishTime");
            sCRestoreDetails.RestoreTo = I18NEntity.GetString(sqlReader["RestoreTo"].ToString());
            sCRestoreDetails.IsDaoMigration = Convert.ToInt32(sqlReader["IsDaoMigration"].ToString());
            sCRestoreDetails.IsEndUserOpt = Convert.ToInt32(sqlReader["ISEndUserOpt"].ToString());
            sCRestoreDetails.Status = sqlReader["Status"] == null ? JobDetailsStatus.None : (JobDetailsStatus)Int32.Parse(sqlReader["Status"].ToString());
            sCRestoreDetails.Comment = sqlReader["Comment"].ToString();
            return sCRestoreDetails;
        }

        public ArchivedSiteReport ConvertDomainToArchivedSiteReport(SQLiteDataReader sqlReader)
        {
            ArchivedSiteReport reportInfo = new ArchivedSiteReport();
            reportInfo.ObjectLevel = sqlReader["ObjectLevel"] == null ? 0 : Int32.Parse(sqlReader["ObjectLevel"].ToString());
            reportInfo.TitleOrName = sqlReader["TitleOrName"].ToString();
            reportInfo.Url = sqlReader["Url"].ToString();
            reportInfo.Type = GetReportColumnValue(sqlReader, "Type");
            reportInfo.SourceUrl = GetReportColumnValue(sqlReader, "SourceUrl");
            reportInfo.ArchivedDataSize = string.IsNullOrEmpty(GetReportColumnValue(sqlReader, "ArchivedDataSize")) ? 0 : Double.Parse(GetReportColumnValue(sqlReader, "ArchivedDataSize"));
            reportInfo.CreatedTime = string.IsNullOrEmpty(GetReportColumnValue(sqlReader, "CreatedTime")) ? 0 : Int64.Parse(GetReportColumnValue(sqlReader, "CreatedTime"));
            reportInfo.LastModifiedTime = string.IsNullOrEmpty(GetReportColumnValue(sqlReader, "LastModifiedTime")) ? 0 : Int64.Parse(GetReportColumnValue(sqlReader, "LastModifiedTime"));
            reportInfo.ArchivedTime = string.IsNullOrEmpty(GetReportColumnValue(sqlReader, "ArchivedTime")) ? 0 : Int64.Parse(GetReportColumnValue(sqlReader, "ArchivedTime"));
            return reportInfo;
        }
        public bool IsExistTable(string reportFilePath, string tableName)
        {
            return SQLCommond.IsExistTable(reportFilePath, tableName);
        }

        public List<string> FilterColumns(string reportFilePath, string tableName, List<string> columnNames)
        {
            return SQLCommond.FilterColumns(reportFilePath, tableName, columnNames);
        }

        /// <summary>
        /// column不存在或者值为null，返回空字符串
        /// </summary>
        /// <param name="sqlReader"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private string GetReportColumnValue(SQLiteDataReader sqlReader, string name)
        {
            var existColumn = -1 != sqlReader.GetOrdinal(name);
            if (existColumn && sqlReader[name] != null)
            {
                return sqlReader[name].ToString();
            }
            return "";
        }

        public List<string> GetDistinctValues(string reportFilePath, string tableName, string columnName)
        {
            List<string> result = new List<string>();
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection("Data Source=" + reportFilePath))
                {
                    conn.Open();
                    try
                    {
                        string query = $"SELECT DISTINCT {SecurityUtils.SanitizeSQLSchemaName(columnName)} FROM {SecurityUtils.SanitizeSQLSchemaName(tableName)}";
                        //Fortify fix: Unreleased Resource: Database
                        using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                        {
                            using (SQLiteDataReader sqlReader = cmd.ExecuteReader())
                            {
                                while (sqlReader.Read())
                                {
                                    result.Add(sqlReader.GetString(0));
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
            catch (Exception e)
            {
                logger.Error(string.Format("Get distinct values failed. {0}", e));
            }
            return result;
        }
    }
}
