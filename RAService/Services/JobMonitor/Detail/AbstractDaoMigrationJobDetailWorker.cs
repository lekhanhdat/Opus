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
using AvePoint.GCommon.Contract.Server.Common.Monitor.Object.Detail;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Server.Job.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.JobMonitor.Util;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Resource;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseJobDto = AvePoint.RA.Contract.JobMonitor.BaseJobDto;

namespace AvePoint.RA.Service.Services.JobMonitor.Detail
{
    abstract public class AbstractDaoMigrationJobDetailWorker : AbstractJobDetailWorker
    {
        private const int retryCount = 5;
        public override void InsertData(IEnumerable<JMJobDetails> jobDetails, BaseJobDto jobInfo)
        {
            throw new NotImplementedException();
        }

        public override string DownloadReports(BaseJobDto jobInfo)
        {
            string tempPath = string.Empty;
            try
            {
                tempPath = JobReportUtility.GetArchiverJobReportPath(jobInfo, ExpandedName);
                if (SQLCommond.CanConnectToReportFile(tempPath))
                {
                    return tempPath;
                }
                RAStorageUtil.DownloadReport4ArchiverJob(jobInfo);
            }
            catch (Exception e)
            {
                logger.Error("download detail file error:{0}", e.ToString());
            }
            return tempPath;
        }

        private string InnerConvertXmlToI18NString(string xmlString)
        {
            if (string.IsNullOrEmpty(xmlString)) return string.Empty;

            if (xmlString.StartsWith("<", StringComparison.Ordinal))
            {
                try
                {
                    List<PropertyItem> PropertyItems = SerializerHelper.DeserializeFromXmlString<List<PropertyItem>>(xmlString);
                    string iI8NStr = string.Empty;
                    foreach (PropertyItem item in PropertyItems)
                    {
                        if (GConstants.JobSummaryKey.Gui_NewLine.Equals(item.Key))//换行
                        {
                            iI8NStr += "\r\n";
                            continue;
                        }
                        try
                        {
                            iI8NStr += item.Args != null && item.Args.Length > 0 ? I18NEntity.GetComment(item.Key, item.DefaultValue, item.Args) : I18NEntity.GetComment(item.Key, item.DefaultValue);
                        }
                        catch (Exception e)
                        {
                            iI8NStr = item.DefaultValue;
                        }
                    }
                    return iI8NStr;
                }
                catch (Exception e)
                {
                    logger.Warn(xmlString + " Deserialize error: " + e.ToString());
                    //if (xmlString.ToLower(CultureInfo.CurrentCulture).Contains("jobsummarycommonts"))
                    //{
                    //    try
                    //    {
                    //        JobSummaryCommonts comments = SerializerHelper.DeserializeFromXmlString<JobSummaryCommonts>(xmlString);
                    //        return I18NEntity.GetComment(comments.Message, comments.Message);
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        logger.Error(xmlString + " Deserialize error: " + ex.ToString());
                    //    }
                    //}
                    //else
                    //{
                    //    try
                    //    {
                    //        return SerializerHelper.DeserializeFromXmlString<object>(xmlString).ToString();
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        logger.Error(xmlString + " Deserialize error: " + ex.ToString());
                    //    }
                    //}
                    return xmlString;
                }
            }
            return I18NEntity.GetComment(xmlString, xmlString);
        }
        protected string ConvertXmlToI18NString(string xmlString)
        {
            return TryConvertWrapperComment(InnerConvertXmlToI18NString(xmlString));
        }
        private string TryConvertWrapperComment(string i18NStr)
        {
            if (i18NStr == WrapperReportResourceKey.Wrapper_SkippedItemByLastModifiedTime.ToString())
            {
                return WrapperRestoreReportResource.Wrapper_SkippedItemByLastModifiedTime;
            }
            else if (i18NStr == WrapperReportResourceKey.Wrapper_SkippedByDeclaredDocument.ToString())
            {
                return WrapperRestoreReportResource.Wrapper_SkippedByDeclaredDocument;
            }
            else if (i18NStr == WrapperReportResourceKey.Wrapper_SkippedItemByHasUniqueValue.ToString())
            {
                return WrapperRestoreReportResource.Wrapper_SkippedItemByHasUniqueValue;
            }
            else if (i18NStr == WrapperReportResourceKey.Wrapper_SkippedByIsPersonalView.ToString())
            {
                return WrapperRestoreReportResource.Wrapper_SkippedByIsPersonalView;
            }
            else if (i18NStr == WrapperReportResourceKey.Wrapper_SkippedByCannotEditItem.ToString())
            {
                return WrapperRestoreReportResource.Wrapper_SkippedByCannotEditItem;
            }
            else if (i18NStr == WrapperReportResourceKey.Wrapper_SkippedItemByIsSameItem.ToString())
            {
                return WrapperRestoreReportResource.Wrapper_SkippedItemByIsSameItem;
            }
            else if (i18NStr == WrapperReportResourceKey.Wrapper_SkippedItemByTargetGtSourceVersion.ToString())
            {
                return WrapperRestoreReportResource.Wrapper_SkippedItemByTargetGtSourceVersion;
            }
            else return i18NStr;
        }

        public List<JobSummary> GetJobSummary(BaseJobDto jobDto, JobReportDetailEntityType[] entitytypes)
        {
            StringBuilder sBuilder = new StringBuilder();
            foreach (JobReportDetailEntityType item in entitytypes)
            {
                sBuilder.Append((int)item).Append(",");
            }
            sBuilder.Remove(sBuilder.Length - 1, 1);

            string sqlString = "select * from JobSummary where EntityType in (" + sBuilder.ToString() + ")";

            return GetJobSummaryReadReportFileWithRetry(jobDto, sqlString);
        }

        public List<JobSummary> GetJobSummaryReadReportFileWithRetry(BaseJobDto jobDto, string sqlString)
        {
            List<JobSummary> summarys = new List<JobSummary>();
            for (int i = 0; i < retryCount; i++)
            {
                try
                {
                    summarys = GetJobSummaryReadReportFile(jobDto, sqlString);
                    break;
                }
                catch
                {
                    System.Threading.Thread.Sleep(1 * 1000);
                    logger.Warn("Retry to get job summary {0} times of job {1}", i + 1, jobDto.Id);
                }
            }
            return summarys;
        }

        private List<JobSummary> GetJobSummaryReadReportFile(BaseJobDto jobInfo, string sqlString)
        {
            bool innererror = false;
            List<JobSummary> jobSummarys = new List<JobSummary>();
            string reportFilePath = DownloadReports(jobInfo);
            TABLE_NAME = "JobSummary";
            bool isRPTExist = CheckFileExist(reportFilePath);
            bool isTableInRPTExist = JobDetailDao.IsExistTable(reportFilePath, TABLE_NAME);
            if (!isRPTExist || !isTableInRPTExist)
            {
                logger.Debug("about {0} database exist:{1},table exist{2}", jobInfo.Id, isRPTExist, isTableInRPTExist);
                return jobSummarys;
            }
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection("Data Source=" + reportFilePath))
                {
                    conn.Open();
                    try
                    {
                        try
                        {
                            using (SQLiteCommand scmd = conn.CreateCommand())
                            {
                                string pragma = "PRAGMA journal_mode = OFF";
                                scmd.CommandText = pragma;
                                scmd.ExecuteNonQuery();
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Error("Journal mode off error:{0}", ex.ToString());
                            innererror = true;
                            conn.Close();
                            throw;
                        }
                        using (SQLiteCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = sqlString;
                            using (SQLiteDataReader sqLite = cmd.ExecuteReader())
                            {
                                while (sqLite.Read())
                                {
                                    try
                                    {
                                        JobSummary jobSummary = new JobSummary();
                                        jobSummary.Key = sqLite["Key"].ToString();
                                        if (string.IsNullOrEmpty(sqLite["Value"].ToString()))
                                        {
                                            jobSummary.Value = "";
                                        }
                                        else
                                        {
                                            if (jobSummary.Key.Equals(GConstants.JobSummaryKey.Comments, StringComparison.Ordinal) ||
                                                jobSummary.Key.Equals(GConstants.JobSummaryKey.CommentForSubJob, StringComparison.Ordinal))
                                            {
                                                //由于comment 可能带有用于国际化的参数，因此需要国际化的时候再进行反序列化以提取Message 和 参数
                                                jobSummary.Value = sqLite["Value"].ToString();
                                                //jobSummary.Value = SerializerHelper.DeserializeFromXmlString<JobSummaryCommonts>(sqLite["Value"].ToString()).Message;
                                            }
                                            else
                                            {
                                                try
                                                {
                                                    jobSummary.Value = SerializerHelper.DeserializeFromXmlString<object>(sqLite["Value"].ToString());
                                                }
                                                catch (Exception e)
                                                {
                                                    jobSummary.Value = sqLite["Value"].ToString();
                                                    logger.Warn("Deserialize From Xml String to object error." + e.ToString());
                                                }
                                            }

                                        }
                                        jobSummary.SubJobId = sqLite["SubJobId"].ToString();
                                        jobSummary.EntityType = Convert.ToInt32(sqLite["EntityType"].ToString());
                                        jobSummarys.Add(jobSummary);
                                    }
                                    catch (Exception e1)
                                    {
                                        logger.Error("Read sqlite error: {0}, {1}", e1.Message, e1);
                                        innererror = true;
                                        conn.Close();
                                        throw;
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        if (!innererror)
                        {
                            logger.Error("Execute sqlite command error: {0}", e.Message);
                            innererror = true;
                            conn.Close();
                        }
                        throw;
                    }
                }
            }
            catch (Exception e)
            {
                if (!innererror)
                {
                    logger.Error("Connect sqlite error: {0}", e.Message);
                }
                throw;
            }
            return jobSummarys;
        }
    }
}
