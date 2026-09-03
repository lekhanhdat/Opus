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



using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Utilities;

namespace AvePoint.ObjectModel.ServerSE
{
    [SuppressMessage("FxCopCustomRules", "C100001:DoNotNameSensitiveWords")]
    class AveConfigurationDatabase : AveDatabase, IAveConfigurationDatabase, IAvePersistedStoreProvider
    {
        private const string mConfigurationDatabase_Type = "Microsoft.SharePoint.Administration.SPConfigurationDatabase";
        private object mConfigurationDatabase;
        private static AveConfigurationDatabase mLocal;

        public AveConfigurationDatabase(object configurationDatabase)
            : base((SPDatabase)configurationDatabase)
        {
            mConfigurationDatabase = configurationDatabase;
        }

        public AveConfigurationDatabase()
            : this(AveAssemblyUtility.CreateInstance(mConfigurationDatabase_Type))
        { }

        public IAveConfigurationDatabase Local
        {
            get
            {
                if (mLocal == null)
                {
                    object configurationDatabase = AveAssemblyUtility.GetStaticPropertyValue(mConfigurationDatabase_Type, "Local");
                    if (configurationDatabase != null)
                    {
                        mLocal = new AveConfigurationDatabase(configurationDatabase);
                    }
                }
                return mLocal;
            }
        }

        public override void Dispose()
        {
            AveAssemblyUtility.InvokeMethod(mConfigurationDatabase, "Dispose", new object[] { });
        }

        private List<AveTimerJobStatus> GetRunningJobs(AveJobsPageInfo pageInfo)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("ObjectModel.Server.AveConfigurationDatabase.GetRunningJobs"))
            {

                List<AveTimerJobStatus> jobs = new List<AveTimerJobStatus>();

                int startIndex = pageInfo.PageSize * (pageInfo.CurPage - 1);
                using (SqlCommand command = new SqlCommand(TimerJobsCollection.RunningJobsSql))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.Add("@ServiceId", SqlDbType.UniqueIdentifier).Value = (!string.IsNullOrEmpty(pageInfo.ServiceId)) ? ((object)new Guid(pageInfo.ServiceId)) : ((object)DBNull.Value);
                    command.Parameters.Add("@WebApplicationId", SqlDbType.UniqueIdentifier).Value = (!string.IsNullOrEmpty(pageInfo.WebAppId)) ? ((object)new Guid(pageInfo.WebAppId)) : ((object)DBNull.Value);
                    command.Parameters.Add("@ServerId", SqlDbType.UniqueIdentifier).Value = (!string.IsNullOrEmpty(pageInfo.ServerId)) ? ((object)new Guid(pageInfo.ServerId)) : ((object)DBNull.Value);
                    command.Parameters.Add("@JobId", SqlDbType.UniqueIdentifier).Value = (!string.IsNullOrEmpty(pageInfo.JobDefinitionId)) ? ((object)new Guid(pageInfo.JobDefinitionId)) : ((object)DBNull.Value);
                    command.Parameters.Add("@StartRow", SqlDbType.Int).Value = startIndex;
                    //int num = pageInfo.PageSize + 1;
                    command.Parameters.Add("@MaximumRows", SqlDbType.Int).Value = pageInfo.PageSize;
                    SPFarm local = SPFarm.Local;
                    int num2 = 0;
                    using (SqlDataReader reader = GetReader(command, CommandBehavior.CloseConnection))
                    {
                        while (reader.Read())
                        {
                            num2++;
                            if (num2 > pageInfo.PageSize)
                            {
                                break;
                            }
                            Guid id = reader.GetGuid(2);
                            Guid guid = reader.GetGuid(4);
                            DateTime dateTime = reader.GetDateTime(6);
                            int progress = reader.GetInt32(9);
                            SPJobDefinition definition = local.GetObject(string.IsNullOrEmpty(pageInfo.JobDefinitionId) ? (id) : (new Guid(pageInfo.JobDefinitionId))) as SPJobDefinition;
                            if (definition == null)
                            {
                                continue;
                            }
                            SPServer server = local.GetObject(string.IsNullOrEmpty(pageInfo.ServerId) ? (guid) : (new Guid(pageInfo.ServerId))) as SPServer;
                            if (server == null)
                            {
                                continue;
                            }
                            //row["ItemName"] = SPHttpUtility.NoEncode(definition.DisplayName);
                            //row["Server"] = SPHttpUtility.NoEncode(server2.DisplayName);
                            //row["DateTime"] = dateTime;
                            //row["StartTimeString"] = SPUtility.FormatDate(SPContext.Current.Web, dateTime, SPDateFormat.DateTime);
                            //if (definition.WebApplication != null)
                            //{
                            //    row["WebApplication"] = SPHttpUtility.NoEncode(definition.WebApplication.DisplayName);
                            //}
                            //table.Rows.Add(row);
                            AveTimerJobStatus job = new AveTimerJobStatus();
                            job.JobTitle = SPHttpUtility.NoEncode(definition.DisplayName);
                            job.Server = SPHttpUtility.NoEncode(server.DisplayName);
                            job.Started = GetDateTimeLongString(dateTime);
                            job.Progress = progress;
                            jobs.Add(job);

                        }
                    }
                    //this.m_totalRowCount = StartRowIndex + num2;
                }
                return jobs;

            }

        }

        private List<AveTimerJobStatus> GetHistoryJobs(AveJobsPageInfo pageInfo)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("ObjectModel.Server.AveConfigurationDatabase.GetHistoryJobs"))
            {

                List<AveTimerJobStatus> jobs = new List<AveTimerJobStatus>();

                int startIndex = pageInfo.PageSize * (pageInfo.CurPage - 1);
                using (SqlCommand command = new SqlCommand(TimerJobsCollection.HistoryJobsSql))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.Add("@ServiceId", SqlDbType.UniqueIdentifier).Value = (!string.IsNullOrEmpty(pageInfo.ServiceId)) ? ((object)new Guid(pageInfo.ServiceId)) : ((object)DBNull.Value);
                    command.Parameters.Add("@WebApplicationId", SqlDbType.UniqueIdentifier).Value = (!string.IsNullOrEmpty(pageInfo.WebAppId)) ? ((object)new Guid(pageInfo.WebAppId)) : ((object)DBNull.Value);
                    command.Parameters.Add("@ServerId", SqlDbType.UniqueIdentifier).Value = (!string.IsNullOrEmpty(pageInfo.ServerId)) ? ((object)new Guid(pageInfo.ServerId)) : ((object)DBNull.Value);
                    command.Parameters.Add("@JobId", SqlDbType.UniqueIdentifier).Value = (!string.IsNullOrEmpty(pageInfo.JobDefinitionId)) ? ((object)new Guid(pageInfo.JobDefinitionId)) : ((object)DBNull.Value);
                    command.Parameters.Add("@StartRow", SqlDbType.Int).Value = startIndex + 1;
                    command.Parameters.Add("@JobStatus", SqlDbType.Int).Value = (pageInfo.Status != 0) ? (pageInfo.Status) : ((object)DBNull.Value);
                    //int num = pageInfo.PageSize + 1;
                    command.Parameters.Add("@MaximumRows", SqlDbType.Int).Value = pageInfo.PageSize;
                    SPFarm local = SPFarm.Local;
                    int num2 = 0;
                    using (SqlDataReader reader = GetReader(command, CommandBehavior.CloseConnection))
                    {
                        while (reader.Read())
                        {
                            num2++;
                            if (num2 > pageInfo.PageSize)
                            {
                                break;
                            }
                            Guid id = reader.GetGuid(1);
                            Guid guid = reader.GetGuid(6);
                            int status = reader.GetInt32(8);
                            DateTime startTime = reader.GetDateTime(9);
                            DateTime endTime = reader.GetDateTime(10);
                            string webAppName = reader.GetString(4);
                            SPJobDefinition definition = local.GetObject(string.IsNullOrEmpty(pageInfo.JobDefinitionId) ? (id) : (new Guid(pageInfo.JobDefinitionId))) as SPJobDefinition;
                            if (definition == null)
                            {
                                continue;
                            }
                            SPServer server = local.GetObject(string.IsNullOrEmpty(pageInfo.ServerId) ? (guid) : (new Guid(pageInfo.ServerId))) as SPServer;
                            if (server == null)
                            {
                                continue;
                            }
                            //row["ItemName"] = SPHttpUtility.NoEncode(definition.DisplayName);
                            //row["Server"] = SPHttpUtility.NoEncode(server2.DisplayName);
                            //row["DateTime"] = dateTime;
                            //row["StartTimeString"] = SPUtility.FormatDate(SPContext.Current.Web, dateTime, SPDateFormat.DateTime);
                            //if (definition.WebApplication != null)
                            //{
                            //    row["WebApplication"] = SPHttpUtility.NoEncode(definition.WebApplication.DisplayName);
                            //}
                            //table.Rows.Add(row);
                            AveTimerJobStatus job = new AveTimerJobStatus();
                            job.JobTitle = SPHttpUtility.NoEncode(definition.DisplayName);
                            job.Server = SPHttpUtility.NoEncode(server.DisplayName);
                            job.Started = (endTime - startTime).Duration().ToString();
                            job.Ended = GetDateTimeLongString(endTime);
                            job.Status = status;
                            job.WebApplication = webAppName;
                            jobs.Add(job);
                        }
                    }
                    //this.m_totalRowCount = StartRowIndex + num2;
                }

                return jobs;

            }

        }

        private List<AveTimerJobStatus> GetScheduledJobs(AveJobsPageInfo pageInfo)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("ObjectModel.Server.AveConfigurationDatabase.GetScheduledJobs"))
            {

                List<AveTimerJobStatus> jobs = new List<AveTimerJobStatus>();
                int startIndex = pageInfo.PageSize * (pageInfo.CurPage - 1);
                using (SqlCommand command = new SqlCommand(TimerJobsCollection.ScheduledJobsSql))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.Add("@ServiceId", SqlDbType.UniqueIdentifier).Value = (!string.IsNullOrEmpty(pageInfo.ServiceId)) ? ((object)new Guid(pageInfo.ServiceId)) : ((object)DBNull.Value);
                    command.Parameters.Add("@WebApplicationId", SqlDbType.UniqueIdentifier).Value = (!string.IsNullOrEmpty(pageInfo.WebAppId)) ? ((object)new Guid(pageInfo.WebAppId)) : ((object)DBNull.Value);
                    command.Parameters.Add("@ServerId", SqlDbType.UniqueIdentifier).Value = (!string.IsNullOrEmpty(pageInfo.ServerId)) ? ((object)new Guid(pageInfo.ServerId)) : ((object)DBNull.Value);
                    command.Parameters.Add("@JobId", SqlDbType.UniqueIdentifier).Value = (!string.IsNullOrEmpty(pageInfo.JobDefinitionId)) ? ((object)new Guid(pageInfo.JobDefinitionId)) : ((object)DBNull.Value);
                    command.Parameters.Add("@StartRow", SqlDbType.Int).Value = startIndex;
                    // int num = pageInfo.PageSize + 1;
                    command.Parameters.Add("@MaximumRows", SqlDbType.Int).Value = pageInfo.PageSize;
                    SPFarm local = SPFarm.Local;
                    int num2 = 0;
                    using (SqlDataReader reader = GetReader(command, CommandBehavior.CloseConnection))
                    {
                        while (reader.Read())
                        {
                            num2++;
                            if (num2 > pageInfo.PageSize)
                            {
                                break;
                            }
                            Guid id = reader.GetGuid(3);
                            Guid guid = reader.GetGuid(2);
                            //Guid webAppId = reader.GetGuid(1);
                            DateTime dateTime = reader.GetDateTime(4);
                            SPJobDefinition definition = local.GetObject(string.IsNullOrEmpty(pageInfo.JobDefinitionId) ? (id) : (new Guid(pageInfo.JobDefinitionId))) as SPJobDefinition;
                            if (definition == null)
                            {
                                continue;
                            }
                            SPServer server = local.GetObject(string.IsNullOrEmpty(pageInfo.ServerId) ? (guid) : (new Guid(pageInfo.ServerId))) as SPServer;
                            if (server == null)
                            {
                                continue;
                            }
                            //SPWebApplication webApplication = local.GetObject(string.IsNullOrEmpty(pageInfo.WebAppId) ? (webAppId) : (new Guid(pageInfo.WebAppId))) as SPWebApplication;

                            //row["ItemName"] = SPHttpUtility.NoEncode(definition.DisplayName);
                            //row["Server"] = SPHttpUtility.NoEncode(server2.DisplayName);
                            //row["DateTime"] = dateTime;
                            //row["StartTimeString"] = SPUtility.FormatDate(SPContext.Current.Web, dateTime, SPDateFormat.DateTime);
                            //if (definition.WebApplication != null)
                            //{
                            //    row["WebApplication"] = SPHttpUtility.NoEncode(definition.WebApplication.DisplayName);
                            //}
                            //table.Rows.Add(row);
                            AveTimerJobStatus job = new AveTimerJobStatus();
                            job.JobTitle = SPHttpUtility.NoEncode(definition.DisplayName);
                            job.Server = SPHttpUtility.NoEncode(server.DisplayName);
                            job.Started = GetDateTimeLongString(dateTime);
                            if (definition.WebApplication != null)
                            {
                                job.WebApplication = SPHttpUtility.NoEncode(definition.WebApplication.DisplayName);
                            }
                            jobs.Add(job);
                        }
                    }
                    //this.m_totalRowCount = StartRowIndex + num2;
                }

                return jobs;

            }

        }

        private string GetDateTimeLongString(DateTime dateTime)
        {
            return dateTime.ToLocalTime().ToString();
        }

        private SqlDataReader GetReader(SqlCommand command, CommandBehavior CloseConnection)
        {
            return this.Local.SqlSession.ExecuteReader(command, CloseConnection);
        }

        public Dictionary<AveJobType, List<AveTimerJobStatus>> GetWebApplicationTimerJobs(AveJobsPageInfo pageInfo)
        {
            Dictionary<AveJobType, List<AveTimerJobStatus>> webAppTimerJobs = new Dictionary<AveJobType, List<AveTimerJobStatus>>();
            switch (pageInfo.JobType)
            {
                case AveJobType.HistoryJob:
                    webAppTimerJobs.Add(AveJobType.HistoryJob, GetHistoryJobs(pageInfo));
                    return webAppTimerJobs;
                case AveJobType.RunningJob:
                    webAppTimerJobs.Add(AveJobType.RunningJob, GetRunningJobs(pageInfo));
                    return webAppTimerJobs;
                case AveJobType.ScheduledJob:
                    webAppTimerJobs.Add(AveJobType.ScheduledJob, GetScheduledJobs(pageInfo));
                    return webAppTimerJobs;
                case AveJobType.AllJob:
                    webAppTimerJobs.Add(AveJobType.HistoryJob, GetHistoryJobs(pageInfo));
                    webAppTimerJobs.Add(AveJobType.RunningJob, GetRunningJobs(pageInfo));
                    webAppTimerJobs.Add(AveJobType.ScheduledJob, GetScheduledJobs(pageInfo));
                    return webAppTimerJobs;
                default:
                    return null;
            }
        }

        public Dictionary<AveJobType, List<AveTimerJobStatus>> GetServiceTimerJobs(AveJobsPageInfo pageInfo)
        {
            Dictionary<AveJobType, List<AveTimerJobStatus>> serviceTimerJobs = new Dictionary<AveJobType, List<AveTimerJobStatus>>();
            switch (pageInfo.JobType)
            {
                case AveJobType.HistoryJob:
                    serviceTimerJobs.Add(AveJobType.HistoryJob, GetHistoryJobs(pageInfo));
                    return serviceTimerJobs;
                case AveJobType.RunningJob:
                    serviceTimerJobs.Add(AveJobType.RunningJob, GetRunningJobs(pageInfo));
                    return serviceTimerJobs;
                case AveJobType.ScheduledJob:
                    serviceTimerJobs.Add(AveJobType.ScheduledJob, GetScheduledJobs(pageInfo));
                    return serviceTimerJobs;
                case AveJobType.AllJob:
                    serviceTimerJobs.Add(AveJobType.HistoryJob, GetHistoryJobs(pageInfo));
                    serviceTimerJobs.Add(AveJobType.RunningJob, GetRunningJobs(pageInfo));
                    serviceTimerJobs.Add(AveJobType.ScheduledJob, GetScheduledJobs(pageInfo));
                    return serviceTimerJobs;
                default:
                    return null;
            }
        }

        public Dictionary<AveJobType, List<AveTimerJobStatus>> GetServerTimerJobs(AveJobsPageInfo pageInfo)
        {
            Dictionary<AveJobType, List<AveTimerJobStatus>> serverTimerJobs = new Dictionary<AveJobType, List<AveTimerJobStatus>>();
            switch (pageInfo.JobType)
            {
                case AveJobType.HistoryJob:
                    serverTimerJobs.Add(AveJobType.HistoryJob, GetHistoryJobs(pageInfo));
                    return serverTimerJobs;
                case AveJobType.RunningJob:
                    serverTimerJobs.Add(AveJobType.RunningJob, GetRunningJobs(pageInfo));
                    return serverTimerJobs;
                case AveJobType.ScheduledJob:
                    serverTimerJobs.Add(AveJobType.ScheduledJob, GetScheduledJobs(pageInfo));
                    return serverTimerJobs;
                case AveJobType.AllJob:
                    serverTimerJobs.Add(AveJobType.HistoryJob, GetHistoryJobs(pageInfo));
                    serverTimerJobs.Add(AveJobType.RunningJob, GetRunningJobs(pageInfo));
                    serverTimerJobs.Add(AveJobType.ScheduledJob, GetScheduledJobs(pageInfo));
                    return serverTimerJobs;
                default:
                    return null;
            }
        }

        public Dictionary<AveJobType, List<AveTimerJobStatus>> GetJobDefinitionTimerJobs(AveJobsPageInfo pageInfo)
        {
            Dictionary<AveJobType, List<AveTimerJobStatus>> jobDefinitionTimerJobs = new Dictionary<AveJobType, List<AveTimerJobStatus>>();
            switch (pageInfo.JobType)
            {
                case AveJobType.HistoryJob:
                    jobDefinitionTimerJobs.Add(AveJobType.HistoryJob, GetHistoryJobs(pageInfo));
                    return jobDefinitionTimerJobs;
                case AveJobType.RunningJob:
                    jobDefinitionTimerJobs.Add(AveJobType.RunningJob, GetRunningJobs(pageInfo));
                    return jobDefinitionTimerJobs;
                case AveJobType.ScheduledJob:
                    jobDefinitionTimerJobs.Add(AveJobType.ScheduledJob, GetScheduledJobs(pageInfo));
                    return jobDefinitionTimerJobs;
                case AveJobType.AllJob:
                    jobDefinitionTimerJobs.Add(AveJobType.HistoryJob, GetHistoryJobs(pageInfo));
                    jobDefinitionTimerJobs.Add(AveJobType.RunningJob, GetRunningJobs(pageInfo));
                    jobDefinitionTimerJobs.Add(AveJobType.ScheduledJob, GetScheduledJobs(pageInfo));
                    return jobDefinitionTimerJobs;
                default:
                    return null;
            }
        }

        public Dictionary<AveJobType, List<AveTimerJobStatus>> GetAllTimerJobs(AveJobsPageInfo pageInfo)
        {
            Dictionary<AveJobType, List<AveTimerJobStatus>> allTimerJobs = new Dictionary<AveJobType, List<AveTimerJobStatus>>();
            switch (pageInfo.JobType)
            {
                case AveJobType.HistoryJob:
                    allTimerJobs.Add(AveJobType.HistoryJob, GetHistoryJobs(pageInfo));
                    return allTimerJobs;
                case AveJobType.RunningJob:
                    allTimerJobs.Add(AveJobType.RunningJob, GetRunningJobs(pageInfo));
                    return allTimerJobs;
                case AveJobType.ScheduledJob:
                    allTimerJobs.Add(AveJobType.ScheduledJob, GetScheduledJobs(pageInfo));
                    return allTimerJobs;
                case AveJobType.AllJob:
                    allTimerJobs.Add(AveJobType.HistoryJob, GetHistoryJobs(pageInfo));
                    allTimerJobs.Add(AveJobType.RunningJob, GetRunningJobs(pageInfo));
                    allTimerJobs.Add(AveJobType.ScheduledJob, GetScheduledJobs(pageInfo));
                    return allTimerJobs;
                default:
                    return null;
            }
        }

        public Dictionary<AveJobType, int> GetTotalJobsCount(AveJobsPageInfo pageInfo)
        {
            List<int> jobsCount = new List<int>();
            Dictionary<AveJobType, int> totalJobsCount = new Dictionary<AveJobType, int>();
            switch (pageInfo.JobType)
            {
                case AveJobType.ScheduledJob:
                    totalJobsCount.Add(AveJobType.ScheduledJob, GetJobsCount(pageInfo, TimerJobsCollection.ScheduledJobsCountSql));
                    return totalJobsCount;
                case AveJobType.RunningJob:
                    totalJobsCount.Add(AveJobType.RunningJob, GetJobsCount(pageInfo, TimerJobsCollection.RunningJobsCountSql));
                    return totalJobsCount;
                case AveJobType.HistoryJob:
                    totalJobsCount.Add(AveJobType.HistoryJob, GetJobsCount(pageInfo, TimerJobsCollection.HistoryJobsCountSql));
                    return totalJobsCount;
                case AveJobType.AllJob:
                    totalJobsCount.Add(AveJobType.ScheduledJob, GetJobsCount(pageInfo, TimerJobsCollection.ScheduledJobsCountSql));
                    totalJobsCount.Add(AveJobType.RunningJob, GetJobsCount(pageInfo, TimerJobsCollection.RunningJobsCountSql));
                    totalJobsCount.Add(AveJobType.HistoryJob, GetJobsCount(pageInfo, TimerJobsCollection.HistoryJobsCountSql));
                    return totalJobsCount;
                default: return null;
            }
        }

        private int GetJobsCount(AveJobsPageInfo pageInfo, string jobType)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("ObjectModel.Server.AveConfigurationDatabase.GetJobsCount"))
            {

                using (SqlCommand command = new SqlCommand(jobType))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.Add("@ServiceId", SqlDbType.UniqueIdentifier).Value = (!string.IsNullOrEmpty(pageInfo.ServiceId)) ? ((object)new Guid(pageInfo.ServiceId)) : ((object)DBNull.Value);
                    command.Parameters.Add("@WebApplicationId", SqlDbType.UniqueIdentifier).Value = (!string.IsNullOrEmpty(pageInfo.WebAppId)) ? ((object)new Guid(pageInfo.WebAppId)) : ((object)DBNull.Value);
                    command.Parameters.Add("@ServerId", SqlDbType.UniqueIdentifier).Value = (!string.IsNullOrEmpty(pageInfo.ServerId)) ? ((object)new Guid(pageInfo.ServerId)) : ((object)DBNull.Value);
                    command.Parameters.Add("@JobId", SqlDbType.UniqueIdentifier).Value = (!string.IsNullOrEmpty(pageInfo.JobDefinitionId)) ? ((object)new Guid(pageInfo.JobDefinitionId)) : ((object)DBNull.Value);
                    using (SqlDataReader reader = GetReader(command, CommandBehavior.CloseConnection))
                    {
                        while (reader.Read())
                        {
                            int totalElement = reader.GetInt32(0);
                            return (totalElement % pageInfo.PageSize) == 0 ? (totalElement / pageInfo.PageSize) : (totalElement / pageInfo.PageSize + 1);
                        }
                        return 0;
                    }
                }

            }

        }

        public void AddSite(Guid siteId, Guid webappId, Guid databaseId, string siteRelativeUrl, bool hostHeaderIsSiteName)
        {
            throw new NotImplementedException();
        }
    }

    class TimerJobsCollection
    {
        public const string RunningJobsSql = " SELECT" +
                        " TimerRunningJobs.ServiceId," +
                        " TimerRunningJobs.VirtualServerId," +
                        " TimerRunningJobs.JobId," +
                        " TimerRunningJobs.JobTitle," +
                        " TimerRunningJobs.ServerId," +
                        " TimerRunningJobs.Status," +
                        " TimerRunningJobs.StartTime," +
                        " TimerRunningJobs.CurrentTarget," +
                        " TimerRunningJobs.TargetCount," +
                        " TimerRunningJobs.CurrentTargetPercentDone" +
                    " FROM" +
                        " TimerRunningJobs INNER JOIN" +
                        " (SELECT" +
                            " JobId," +
                            " ServerId," +
                            " ROW_NUMBER() OVER (ORDER BY StartTime ASC) AS RowNumber" +
                        " from" +
                            " TimerRunningJobs" +
                        " WHERE" +
                            " (@ServiceId IS NULL OR ServiceId = @ServiceId) AND" +
                            " (@WebApplicationId IS NULL OR VirtualServerId = @WebApplicationId) AND" +
                            " (@ServerId IS NULL OR ServerId = @ServerId) AND" +
                            " (@JobId IS NULL OR JobId = @JobId)" +
                        " ) OrderedJobs ON" +
                            " OrderedJobs.JobId = TimerRunningJobs.JobId AND" +
                            " OrderedJobs.ServerId = TimerRunningJobs.ServerId" +
                    " WHERE 	RowNumber > @StartRow AND RowNumber <= (@StartRow + @MaximumRows)";

        public const string HistoryJobsSql = " SELECT TOP(@MaximumRows)" +
                         " TimerJobHistory.Id," +
                         " JobId," +
                         " JobTitle," +
                         " WebApplicationId," +
                         " WebApplicationName," +
                         " ServiceId," +
                         " ServerId," +
                         " ServerName," +
                         " Status," +
                         " StartTime," +
                         " EndTime," +
                         " DatabaseName," +
                         " ErrorMessage" +
                     " FROM" +
                         " TimerJobHistory" +
                     " WHERE" +
                         " (@ServiceId IS NULL OR ServiceId = @ServiceId) AND" +
                         " (@WebApplicationId IS NULL OR WebApplicationId = @WebApplicationId) AND" +
                         " (@ServerId IS NULL OR ServerId = @ServerId) AND" +
                         " (@JobId IS NULL OR JobId = @JobId) AND" +
                         " (@JobStatus IS NULL OR Status = @JobStatus) AND" +
                         " (TimerJobHistory.Id >= (SELECT MAX(t.Id) FROM (SELECT TOP(@StartRow) Id FROM TimerJobHistory ORDER BY Id ASC) AS t))" +
                     " ORDER BY EndTime DESC";

        public const string ScheduledJobsSql = " SELECT" +
                        " TimerScheduledJobs.ServiceId," +
                        " TimerScheduledJobs.WebApplicationId," +
                        " TimerScheduledJobs.ServerId," +
                        " TimerScheduledJobs.JobId," +
                        " TimerScheduledJobs.StartTime" +
                    " FROM" +
                        " TimerScheduledJobs INNER JOIN" +
                    " (SELECT" +
                        " JobId," +
                        " ServerId," +
                        " ROW_NUMBER() OVER (ORDER BY StartTime ASC) AS RowNumber " +
                     " FROM TimerScheduledJobs" +
                     " WHERE" +
                        " (@ServiceId IS NULL OR ServiceId = @ServiceId) AND" +
                        " (@WebApplicationId IS NULL OR WebApplicationId = @WebApplicationId) AND" +
                        " (@ServerId IS NULL OR ServerId = @ServerId) AND" +
                        " (@JobId IS NULL OR JobId = @JobId)" +
                     " ) OrderedJobs ON" +
                        " OrderedJobs.JobId = TimerScheduledJobs.JobId AND" +
                        " OrderedJobs.ServerId = TimerScheduledJobs.ServerId " +
                    " WHERE  RowNumber > @StartRow AND RowNumber <= (@StartRow + @MaximumRows)";

        public const string ScheduledJobsCountSql = "select count(jobId) from TimerScheduledJobs" +
                " WHERE" +
                 " (@ServiceId IS NULL OR ServiceId = @ServiceId) AND" +
                 " (@WebApplicationId IS NULL OR WebApplicationId = @WebApplicationId) AND" +
                 " (@ServerId IS NULL OR ServerId = @ServerId) AND" +
                 " (@JobId IS NULL OR JobId = @JobId)";

        public const string HistoryJobsCountSql = "select count(jobId) from TimerJobHistory" +
                " WHERE" +
                 " (@ServiceId IS NULL OR ServiceId = @ServiceId) AND" +
                 " (@WebApplicationId IS NULL OR WebApplicationId = @WebApplicationId) AND" +
                 " (@ServerId IS NULL OR ServerId = @ServerId) AND" +
                 " (@JobId IS NULL OR JobId = @JobId)";

        public const string RunningJobsCountSql = "select count(jobId) from TimerRunningJobs" +
                " WHERE" +
                 " (@ServiceId IS NULL OR ServiceId = @ServiceId) AND" +
                 " (@WebApplicationId IS NULL OR VirtualServerId = @WebApplicationId) AND" +
                 " (@ServerId IS NULL OR ServerId = @ServerId) AND" +
                 " (@JobId IS NULL OR JobId = @JobId)";
    }
}
