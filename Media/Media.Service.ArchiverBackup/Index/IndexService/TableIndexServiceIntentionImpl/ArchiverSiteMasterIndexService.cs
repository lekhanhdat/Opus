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




namespace AvePoint.Media.Service.ArchiverBackup
{
    #region using directives

    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using AvePoint.GCommon;
    using AvePoint.GCommon.Contract.CodeReview;
    using AvePoint.GCommon.Contract.StorageOptimization.Archiver;
    using Merged18NResources.MediaServiceArchiverBackup;
    using AvePoint.Media.Service.DomainModel;
    using AvePoint.GCommon.Contract.StorageOptimization.Object;
    using AvePoint.RA.DB.Dao.Impl;
    using AvePoint.RA.DB.Dao;
    using AvePoint.RA.Common;
    using AvePoint.RA.RACommonUtility.Encryption;
    using Google.Apis.Storage.v1;
    using AvePoint.Cryptography;
    using AvePoint.RA.Contract.RMWeb.JobMonitor;
    using AvePoint.RA.Contract.RMWeb.CP;
    using AvePoint.RA.Contract.RMWeb;
    using AvePoint.RA.I18N.Core;
    using Newtonsoft.Json;
    using AvePoint.RA.Contract.JobMonitor;
    using DocumentFormat.OpenXml.Spreadsheet;
    using DocumentFormat.OpenXml.Wordprocessing;
    using System.Linq.Expressions;
    using AvePoint.RA.Common.SystemSetting;
    using AvePoint.RA.Contract.ManualApproval.Model;
    using AvePoint.RA.Common.Util;
    using System.Linq;

    #endregion using directives

    [AveCodeReview(
    "2012/8/2",
    "dwxue@avepoint.com",
    "yjhuo@avepoint.com",
    new string[] { CodeReviewConstants.CHECK_LIST_ID_EH_5 },
    "ADO-44845",
    true)]

    public class ArchiverSiteMasterIndexService
        : ArchiverTableIndexServiceBase
        , IArchiverSiteMasterIndexService
    {
        AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public IMArchiverSiteMasterIndexService SiteMasterIndexService { get; set; }
        public IArchiverSiteMasterIndexDao ArchiverSiteMasterIndexDao=>PlatformWindsorManager.GetService<IArchiverSiteMasterIndexDao>();
        public IArchiverIndexSubInfoDao ArchiverIndexSubInfoDao => PlatformWindsorManager.GetService<IArchiverIndexSubInfoDao>();
        private IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();
        private IRMSubJobDao SubJobDao => PlatformWindsorManager.GetService<IRMSubJobDao>();
        public IJobMonitorDao JMDao => PlatformWindsorManager.GetService<IJobMonitorDao>();
        private readonly static object MLock = new object();
        public void InsertSiteMaster(ArchiverSiteMasterIndex siteMasterIndex)
        {
            this.IndexProcessor.Insert<ArchiverSiteMasterIndex>(siteMasterIndex);
        }

        public Int32 GetSPVersionBySiteCollection(String siteCollection)
        {
            var parameterDictionary = new Dictionary<String, Object>();
            parameterDictionary["@SiteUrl"] = siteCollection;
            var sql = "select distinct COL_SP_VERSION from "
                + IndexConstants.TableNameArchiveSiteMaster
                + " where COL_SITE_URL = @SiteUrl "
                + "order by COL_BACKUP_TIME desc";
            var index = this.IndexProcessor.ExecuteQuery<ArchiverSiteMasterIndex>(sql, parameterDictionary);
            return index.Count > 0 ? index[0].SPVersion : 0;
        }

        public List<ArchiverSiteMasterIndex> GetAllSiteMasterIndex()
        {
            var parameterDictionary = new Dictionary<String, Object>();
            var sql = "select * from " + IndexConstants.TableNameArchiveSiteMaster;
            return this.IndexProcessor.ExecuteQuery<ArchiverSiteMasterIndex>(sql, parameterDictionary);
        }

        public Int64 GetRetentionTimeSpanByJobId(String jobId)
        {
            Int64 result = -1;
            var retentionTimeMap = new Dictionary<String, Int64>();
            var subJobId = jobId.Remove(jobId.LastIndexOf('_'));
            var parameters = new Dictionary<String, Object>();
            parameters["@JobId"] = jobId;
            var selectSql = "select COL_MARK5 from " + IndexConstants.TableNameArchiveSiteMaster + " where COL_JOB_ID = @JobId";
            var updateSql = "update " + IndexConstants.TableNameArchiveSiteMaster + " set COL_MARK5 = @RetentionTime where COL_JOB_ID = @JobId";
            try
            {
                result = (Int64)this.IndexProcessor.ExecuteScalar(selectSql, parameters);
            }
            catch (InvalidCastException e)
            {
                this.logger.Warn(MediaServiceArchiverBackupResource.ArchiverSiteMasterIndexServiceGetRetentionTimeSpanByJobIdWarn, e.ToString());
                retentionTimeMap = this.SiteMasterIndexService.GetRetentionTimeByJobId(subJobId);
                if (retentionTimeMap.ContainsKey(jobId))
                {
                    result = retentionTimeMap[jobId];
                    //6.0中的数据如果没有设置retention，retention time为0
                    if (result == 0)
                        result = -1;
                }
                else
                {
                    result = -1;
                    retentionTimeMap.Add(jobId, -1);
                }
                foreach (KeyValuePair<String, Int64> pair in retentionTimeMap)
                {
                    parameters.Clear();
                    parameters["@RetentionTime"] = pair.Value;
                    parameters["@JobId"] = pair.Key;
                    this.IndexProcessor.Execute(updateSql, parameters);
                }
            }
            return result;
        }

        public ArchiverSiteMasterIndexContract GetSiteCollectionInfo(ArchiverSiteMasterIndexContract site)
        {
            ArchiverSiteMasterIndexContract index = ArchiverSiteMasterIndexDao.GetSiteCollectionInfo(site);
            //if (index != null)
            //{
            //    index.SubInfo = ArchiverIndexSubInfoDao.GetSubInfosLikeJobId(index.JobId);
            //}
            return index;
        }
        public ArchiverSiteMasterIndexContract GetSiteCollectionStorageInfo(ArchiverSiteMasterIndexContract site)
        {
            lock (MLock)
            {
                ArchiverSiteMasterIndexContract result = null;
                List<ArchiverSiteMasterIndexContract> infos = new List<ArchiverSiteMasterIndexContract>();

                infos = ArchiverSiteMasterIndexDao.GetSiteCollectionStorageInfo(site);
                if (infos != null && infos.Count > 0)
                {
                    result = infos[0];
                    result.SubInfo = new List<ArchiverIndexSubInfoContract>();
                    foreach (ArchiverSiteMasterIndexContract info in infos)
                    {
                        List<ArchiverIndexSubInfoContract> subinfos = ArchiverIndexSubInfoDao.GetSubInfoesBySubJobId(info.JobId);
                        foreach (ArchiverIndexSubInfoContract sub in subinfos)
                        {
                            if (!result.SubInfo.Any(a => a.CurrentStorageId == sub.CurrentStorageId))
                            {
                                result.SubInfo.Add(sub);
                            }
                        }
                    }
                }
                else
                {
                    logger.Warn("No storage info got from sub info table job with siteurl: {0}.", site.SiteURL);
                }
                return result;
            }
        }

        public List<string> GetExistingSiteCollectionUrls(IEnumerable<string> siteUrls)
        {
            return ArchiverSiteMasterIndexDao.GetExistingSiteCollectionUrls(siteUrls);
        }
        public List<ArchiverSiteMasterIndexContract> GetSiteCollectionWithSubInfos(ArchiverSiteMasterIndexContract index)
        {
            List<ArchiverSiteMasterIndexContract> result = new List<ArchiverSiteMasterIndexContract>();
            List<ArchiverSiteMasterIndexContract> siteCollections = ArchiverSiteMasterIndexDao.GetSiteCollectionStorageInfo(index);
            if (!siteCollections.IsNullOrEmpty())
            {
                foreach (ArchiverSiteMasterIndexContract siteCollection in siteCollections)
                {
                    List<ArchiverIndexSubInfoContract> subInfos = ArchiverIndexSubInfoDao.GetSubInfoesBySubJobId(siteCollection.JobId);
                    if (!subInfos.IsNullOrEmpty())
                    {
                        siteCollection.SubInfo = subInfos;
                        result.Add(siteCollection);
                    }
                    else
                    {
                        logger.Info("Sub info for node {0} is null or empty.", siteCollection.SiteURL);
                    }
                }
            }
            return result;
        }
        public List<ArchiverSiteMasterIndexContract> GetGoogleDriveWithSubInfos(ArchiverSiteMasterIndexContract index)
        {
            List<ArchiverSiteMasterIndexContract> result = new List<ArchiverSiteMasterIndexContract>();
            List<ArchiverSiteMasterIndexContract> siteCollections = ArchiverSiteMasterIndexDao.GetGDriveStorageInfo(index);
            if (!siteCollections.IsNullOrEmpty())
            {
                foreach (ArchiverSiteMasterIndexContract siteCollection in siteCollections)
                {
                    List<ArchiverIndexSubInfoContract> subInfos = ArchiverIndexSubInfoDao.GetSubInfoesBySubJobId(siteCollection.JobId);
                    if (!subInfos.IsNullOrEmpty())
                    {
                        siteCollection.SubInfo = subInfos;
                        result.Add(siteCollection);
                    }
                    else
                    {
                        logger.Info("Sub info for node {0} is null or empty.", siteCollection.SiteURL);
                    }
                }
            }
            return result;
        }
        public Task BulkCopySiteMasterIndexesAsync(IEnumerable<ArchiverSiteMasterIndexContract> items)
        {
            return ArchiverSiteMasterIndexDao.CreateByBulkCopyAsync(items);
        }
        
        public Task BulkCopyIndexSubInfoesAsync(IEnumerable<ArchiverIndexSubInfoContract> items)
        {
            return ArchiverIndexSubInfoDao.CreateByBulkCopyAsync(items);
        }

        public Task<int> DeleteMigratedSiteMasterIndexesAsync()
        {
            return ArchiverSiteMasterIndexDao.DeleteMigratedSiteMasterIndexesAsync();
        }
        
        public Task<int> DeleteMigratedIndexSubInfoesAsync()
        {
            return ArchiverIndexSubInfoDao.DeleteMigratedIndexSubInfoesAsync();
        }
        public async Task<string> GetFailedJobsDataAsync(JMPager pager)
        {
            JMPageResult responseResult = new JMPageResult();
            responseResult.Result = new List<JMItemInfo>();
            Expression queryExpr = null;
            List<Expression> allExpressionList = new List<Expression>();
            ParameterExpression param = Expression.Parameter(typeof(RA.DB.Model.RMSubJob), "c");
            responseResult.Result = new List<JMItemInfo>();
            List<string> timeFrameColumns = new List<string> { "StartTime", "EndTime" };
            try
            {
                string runBy = "AvePoint Cloud Records System";
                logger.Info("this query is DeleteOrphanDatas query");
                var newValues = new List<string>();
                foreach (var f in pager.Filters)
                {
                    if (timeFrameColumns.Contains(f.ColumnName) && f.ColumnValues != null)
                    {
                        var timeZoneId = (await GeneralSettingService.GetGeneralSettingAsync()).TimeZoneId;
                        var timeZone = GeneralSettingConfig.FindSystemTimeZoneById(timeZoneId);

                        var timeFrame = JsonConvert.DeserializeObject<ManualApprovalTimeFrame>(f.ColumnValues.FirstOrDefault());
                        var endTime = new DateTime(timeFrame.EndTime.Year, timeFrame.EndTime.Month, timeFrame.EndTime.Day, 23, 59, 59); //ensure the EndTime is end of the day.

                        var startTimeTicks = TimeZoneInfo.ConvertTimeToUtc(timeFrame.StartTime, timeZone).Ticks;
                        var endTimeTicks = TimeZoneInfo.ConvertTimeToUtc(endTime, timeZone).Ticks;

                        logger.Info($"Select the data with the time frame between [{startTimeTicks} and {endTimeTicks}]");
                        var exp1 = Expression4DynamicQuery.GetGreaterThanOrEqualExpression(typeof(RA.DB.Model.RMSubJob), param, f.ColumnName, startTimeTicks);
                        var exp2 = Expression4DynamicQuery.GetLessThanOrEqualExpression(typeof(RA.DB.Model.RMSubJob), param, f.ColumnName, endTimeTicks);
                        allExpressionList.AddRange(new List<Expression> { exp1, exp2 });
                        continue;
                    }

                    if (f.ColumnName.Equals("UserName", StringComparison.OrdinalIgnoreCase))
                    {


                        f.ColumnValues.ForEach(v =>
                        {
                            //Verify whether the user wanna query with job run by field is "System".
                            if (v.Equals(I18NEntity.GetString("RM_TS_RunSchedule"), StringComparison.OrdinalIgnoreCase))
                            {
                                newValues.AddRange(new List<string> { "RM_TS_RunSchedule", "AvePoint Cloud Records System" });
                            }
                            else
                            {
                                newValues.Add(v);
                            }
                        });
                        continue;
                    }

                    var exps = f.ColumnValues.Select(c => Expression4DynamicQuery.GetEqualExpression(typeof(RA.DB.Model.RMSubJob), param, f.ColumnName, c));
                    var filterExpression = exps.Aggregate(Expression.OrElse);
                    allExpressionList.Add(filterExpression);
                }
                if (!string.IsNullOrEmpty(pager.SearchValue))
                {
                    try
                    {
                        var exps = pager.SearcheKeys.Select(searchKey => Expression4DynamicQuery.GetContainsExpression(typeof(RA.DB.Model.RMSubJob), param, searchKey, pager.SearchValue));
                        var searchExpression = exps.Aggregate(Expression.OrElse);
                        allExpressionList.Add(searchExpression);
                    }
                    catch (Exception ex)
                    {
                        logger.Warn("{0}", ex.Message.ToString());
                        responseResult.Pager = new JMPager() { TotalNumber = 0, PageSize = 0 };
                        return JsonConvert.SerializeObject(responseResult);
                    }
                }
                List<RA.DB.Model.RMSubJob> failedSubjobs = null;
                if (allExpressionList.Count > 0)
                {
                    logger.Info("this query has filter");
                    queryExpr = allExpressionList.Aggregate(Expression.AndAlso);
                    var lambda = Expression.Lambda<Func<RA.DB.Model.RMSubJob, bool>>(queryExpr, param);
                    failedSubjobs = SubJobDao.GetFailedSubJobs(lambda);
                }
                else
                {
                    logger.Info("this query not has filter");
                    failedSubjobs = SubJobDao.GetFailedSubJobs();
                }
                responseResult.TotalNumber = failedSubjobs.Count;
                failedSubjobs = failedSubjobs.Skip((pager.JumpPage - 1) * pager.PageSize).Take(pager.PageSize).ToList();
                GeneralSettingModel gls = await GeneralSettingService.GetGeneralSettingAsync();
                foreach (var r in failedSubjobs)
                {
                    var job = r;
                    var masterIndex = ArchiverSiteMasterIndexDao.GetIndexByJobId(job.Id);
                    var jobMonitor = JMDao.GetJobById(job.Id.Substring(0, job.Id.LastIndexOf("_")));
                    logger.Info($"get failed job:{job.Id},job type:{job.JobType},status:{job.Status},progress:{job.Progress},starttime:{job.StartTime},endtime:{job.EndTime}");
                    if (jobMonitor !=null && (jobMonitor?.UserName == "RM_TS_RunSchedule" || jobMonitor.UserName.ToLowerInvariant().Equals(runBy.ToLowerInvariant())))
                    {
                        jobMonitor.UserName = I18NEntity.GetString("RM_TS_RunSchedule");
                    }
                    if (jobMonitor?.UserName == "RM_JS_Common_Pending")
                    {
                        jobMonitor.UserName = I18NEntity.GetString("RM_JS_Common_Pending");
                    }
                    if (newValues.Count>0 && !newValues.Contains(jobMonitor?.UserName))
                    {
                        logger.Warn($"this job is not fit filter,skip add it,{job.Id}");
                        continue;
                    }
                    responseResult.Result.Add(new JMItemInfo()
                    {
                        JobId = job.Id,
                        JobTypeCode = job.JobType,
                        JobType = GetJobTypeName(job.JobType),
                        Status = (RA.Contract.RMWeb.JobMonitor.JobStatus)job.Status,
                        Progress = (int)job.Progress,
                        StartTime = job.StartTime == 0 ? "" : GeneralSettingService.ConvertTiksToDateTime(gls, job.StartTime, true).SimplifyFormatTime,
                        EndTime = job.EndTime == 0 ? I18NEntity.GetString("RM_JS_JM_EndTimePending") : GeneralSettingService.ConvertTiksToDateTime(gls, job.EndTime, true).SimplifyFormatTime,
                        UserName = jobMonitor?.UserName,
                        Joblocation = string.IsNullOrEmpty(masterIndex?.FirstOrDefault()?.SiteURL)?r.String1: masterIndex?.FirstOrDefault()?.SiteURL,
                    });
                }
                return JsonConvert.SerializeObject(responseResult);
            }
            catch (Exception ex)
            {
                logger.Error($"get failed job faied,so can not clean it,error:{ex}");
                responseResult.Pager = new JMPager() { TotalNumber = 0, PageSize = 0 };
                return JsonConvert.SerializeObject(responseResult);
            }
        }
        private string GetJobTypeName(int jobtype)
        {
            if (jobtype == (int)JobType.MailBoxBackup)
            {
                return I18NEntity.GetString("RM_JS_JM_JobType_" + JobType.TeamsArchiverBackup.ToString()); 
            }
            return I18NEntity.GetString("RM_JS_JM_JobType_" + ((JobType)jobtype).ToString());
        }

        public ArchiverSiteMasterIndexContract GetGoogleDriveInfo(ArchiverSiteMasterIndexContract site)
        {
            ArchiverSiteMasterIndexContract index = ArchiverSiteMasterIndexDao.GetGoogleDriveInfo(site);
            return index;
        }

        public async Task<(string, string, string)> GetArchivedChannelSiteInfoAsync(string siteCollectionUrl)
        {
            using var _ = new PerformanceScope("ArchiverSiteMasterIndexService.GetArchivedChannelSiteInfoAsync");
            return await ArchiverSiteMasterIndexDao.GetArchivedChannelSiteInfoAsync(siteCollectionUrl);
        }
    }
}