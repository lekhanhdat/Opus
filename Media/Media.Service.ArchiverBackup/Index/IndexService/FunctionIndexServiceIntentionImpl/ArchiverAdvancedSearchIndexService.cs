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
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using AvePoint.GCommon.Contract.CommonFilter;
    using AvePoint.Media.Core.Index;
    using AvePoint.Media.Service.DomainModel;
    using AvePoint.RA.CommonUtil;
    using Cloud.Sdk.Data.Aos;
    using global::Media.Service.DomainModel;

    #endregion using directives

    public class ArchiverAdvancedSearchIndexService
        : ArchiverIndexServiceBase
        , IArchiverAdvancedSearchIndexService
    {
        static readonly String selectSection = "SELECT COL_ID, COL_ARCHIVE_TIME, COL_AUTHOR, COL_EXTENSION_7, COL_TYPE, COL_NAME, COL_PATH_MD5, COL_PARENT_PATH_MD5, COL_ATTRIBUTES, COL_JOBID, COL_CREATE_TIME, COL_MODIFY_TIME,COL_SITE_PATH,COL_EXTENSION_9,COL_EXTENSION_5,COL_EXTRAINFO,COL_EXTENSION_2,COL_RETENTION_STATUS";
        static readonly String selectSectionForContainer = "SELECT COL_ID, MAX(COL_ARCHIVE_TIME) AS COL_ARCHIVE_TIME, COL_AUTHOR, COL_EXTENSION_7, COL_TYPE, COL_NAME, COL_PATH_MD5, COL_PARENT_PATH_MD5, COL_ATTRIBUTES, COL_JOBID, COL_CREATE_TIME, COL_MODIFY_TIME,COL_SITE_PATH,COL_EXTENSION_9,COL_EXTENSION_5,COL_EXTRAINFO,COL_EXTENSION_2,COL_RETENTION_STATUS";
        static readonly String selectSectionForFS = "SELECT MAX(COL_ARCHIVE_TIME) AS COL_ARCHIVE_TIME, COL_EXTENSION_7, COL_TYPE, COL_NAME, COL_PATH_MD5, COL_PARENT_PATH_MD5, COL_ATTRIBUTES, COL_JOBID, COL_CREATE_TIME, COL_MODIFY_TIME,COL_SITE_PATH,COL_EXTENSION_9,COL_EXTENSION_5,COL_EXTRAINFO,COL_EXTENSION_2,COL_RETENTION_STATUS";
        static readonly String selectSectionForJob = "SELECT COL_TYPE, COL_NAME from TB_BODY_INDEX where COL_TYPE = 'D'";
        private RALogger logger = RALogger.GetInstance(typeof(ArchiverAdvancedSearchIndexService));
        public ISqlBuilder _SqlBuilder { get; set; }
        public ISqlBuilder SqlBuilder
        {
            get
            {
                if (_SqlBuilder == null)
                {
                    _SqlBuilder = new ArchiverSqlBuilder();
                    return _SqlBuilder;
                }
                else
                {
                    return _SqlBuilder;
                }
            }
            set { }
        }
        public ArchiverBasicIndex GetParentFolder(ArchiverBasicIndex index)
        {
            return this.HeadAndBodyService.GetParentDataFromHeadTable(index);
        }

        public ArchiverBasicIndex GetItem(string path, long endTime)
        {
            return this.HeadAndBodyService.GetOneDataFromHeadOrBodyTable(path, endTime);
        }

        public List<ArchiverBasicIndex> Search(ArchiverRestoreFilter filter, ArchiverBrowseInfo restoreParam, ArchiverRestoreOrderBy orderBy, out long totalCount)
        {
            var result = new List<ArchiverBasicIndex>();
            var tempList = new List<ArchiverBasicIndex>();
            var sql = new StringBuilder();
            var parameters = new Dictionary<String, Object>();
            SqlBuildInfo buildInfo = null;
            if (filter.Level != PolicyLevel.Document && filter.Level != PolicyLevel.DocumentVersion && filter.Level != PolicyLevel.Item)
            {
                buildInfo = new SqlBuildInfo(selectSectionForContainer, filter);
            }
            else
            {
                buildInfo = new SqlBuildInfo(selectSection, filter);
            }
            sql = SqlBuilder.Build(buildInfo);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            result = this.HeadAndBodyService.GetAllDatasFromHeadOrBodyTableByType(sql, filter, restoreParam, orderBy);
            sw.Stop();
            logger.Info($"serch result that need return count is {result.Count},cost time:{sw.ElapsedMilliseconds}");
            if(filter.IsShowTotalCount)
            {
                totalCount = 1;
                var countSql = new StringBuilder();
                countSql = SqlBuilder.BuildQueryCount(buildInfo);
                sw.Reset();
                sw.Start();
                totalCount = this.HeadAndBodyService.GetTotalCountDataFromHeadOrBodyTableByType(countSql, filter, restoreParam, orderBy);
                sw.Stop();
                logger.Info($"Get total count result that need return count is {result.Count},cost time:{sw.ElapsedMilliseconds}");
            }
            else
                totalCount = 0;
            return result;
        }
        public List<ArchiverBasicIndex> SearchForFS(ArchiverRestoreFilter filter, ArchiverBrowseInfo restoreParam, ArchiverRestoreOrderBy orderBy)
        {
            var result = new List<ArchiverBasicIndex>();
            var sql = new StringBuilder();
            var buildInfo = new SqlBuildInfo(selectSectionForFS, filter);
            sql = SqlBuilder.BuildQueryForFS(buildInfo);
            result = this.HeadAndBodyService.GetAllFSDatasFromHeadOrBodyTableByType(sql, filter, restoreParam, orderBy);
            logger.Info($"serch fs result that need return count is {result.Count}");
            return result;
        }
        public List<ArchiverBasicIndex> SearchForJob(ArchiverBrowseInfo restoreParam)
        {
            var result = new List<ArchiverBasicIndex>();
            var tempList = new List<ArchiverBasicIndex>();
            var sql = new StringBuilder();
            var parameters = new Dictionary<String, Object>();
            var buildInfo = new SqlBuildInfo(selectSectionForJob, new ArchiverRestoreFilter() { Level = PolicyLevel.Document});
            //sql = SqlBuilder.Build(buildInfo);
            result = this.HeadAndBodyService.GetAllDatasFromHeadOrBodyTableByTypeForJob(selectSectionForJob, restoreParam);
            logger.Info($"serch result that need return count is {result.Count}");
            return result;
        }

        public DashBoardInfo SearchForJobV2()
        {
            DashBoardInfo dashBoardInfo = new DashBoardInfo();
            dashBoardInfo.DocumentCount = this.HeadAndBodyService.GetFileCount();
            dashBoardInfo.VersionCount = this.HeadAndBodyService.GetFileVersionCount();
            dashBoardInfo.VersionNumber = (double)dashBoardInfo.VersionCount / 1000;
            dashBoardInfo.FileNumber = (double)dashBoardInfo.DocumentCount / 1000;
            logger.Info($"SearchForJobV2.DocumentCount:{dashBoardInfo.DocumentCount}.VersionCount:{dashBoardInfo.VersionCount}.VersionNumber:{dashBoardInfo.VersionNumber}.FileNumber:{dashBoardInfo.FileNumber}.");
            return dashBoardInfo;
        }

        public long SearchArchivedSizeForExportSubSite(string subSiteUrl, ArchiverBrowseInfo info)
        {
            return this.HeadAndBodyService.GetSubSiteArchiveSize(subSiteUrl, info);
        }

        public async IAsyncEnumerable<ArchiverBasicIndex> SearchForExportAllItemAsync()
        {
            const int pageSize = 2000;

            int headOffset = 0;
            while (true)
            {
                var headPage = this.HeadAndBodyService.GetHeadIndexPage(pageSize, headOffset);
                if (headPage == null)
                {
                    break;
                }

                logger.Info($"SearchForExportAllItemAsync. Get head index page with offset {headOffset}, got {headPage.Count} items.");

                foreach (var item in headPage)
                {
                    yield return item;
                }

                if (headPage.Count < pageSize)
                {
                    logger.Info($"SearchForExportAllItemAsync. Finished searching head index, total items got {headOffset + headPage.Count}. Now start searching body index.");
                    break;
                }

                headOffset += headPage.Count;
                await Task.Yield();
            }
            logger.Info($"SearchForExportAllItemAsync. Finished searching head index, start searching body index.");

            int bodyOffset = 0;
            while (true)
            {
                var bodyPage = this.HeadAndBodyService.GetBodyIndexPage(pageSize, bodyOffset);
                if (bodyPage == null)
                {
                    break;
                }

                logger.Info($"SearchForExportAllItemAsync. Get body index page with offset {bodyOffset}, got {bodyPage.Count} items.");

                foreach (var item in bodyPage)
                {
                    yield return item;
                }

                if (bodyPage.Count < pageSize)
                {
                    logger.Info($"SearchForExportAllItemAsync. Finished searching body index, total items got {bodyOffset + bodyPage.Count}. Search complete.");
                    break;
                }

                bodyOffset += bodyPage.Count;
                await Task.Yield();
            }
            logger.Info($"SearchForExportAllItemAsync. Finished searching body index, search complete.");
        }

        public async IAsyncEnumerable<ArchiverBasicIndex> SearchForExportAllItemOnSpecificTimeRangeAsync(ArchiverBrowseInfo info)
        {
            const int pageSize = 2000;

            int headOffset = 0;
            while (true)
            {
                var headPage = this.HeadAndBodyService.GetHeadIndexPage(pageSize, headOffset)
                    ?.Where(index => index.ArchiveTime <= info.EndTime && index.ArchiveTime >= info.StartTime)
                    .ToList();

                if (headPage == null)
                {
                    break;
                }

                foreach (var item in headPage)
                {
                    yield return item;
                }

                if (headPage.Count < pageSize)
                {
                    break;
                }

                headOffset += headPage.Count;
                await Task.Yield();
            }

            int bodyOffset = 0;
            while (true)
            {
                var bodyPage = this.HeadAndBodyService.GetBodyIndexPage(pageSize, bodyOffset)
                    ?.Where(index => index.ArchiveTime <= info.EndTime && index.ArchiveTime >= info.StartTime)
                    .ToList();

                if (bodyPage == null || bodyPage.Count == 0)
                {
                    break;
                }

                foreach (var item in bodyPage)
                {
                    yield return item;
                }

                if (bodyPage.Count < pageSize)
                {
                    break;
                }

                bodyOffset += bodyPage.Count;
                await Task.Yield();
            }
        }

        public List<ArchiverBasicIndex> SearchSubsitesForExportAllSubSites(ArchiverBrowseInfo info)
        {
            List<ArchiverBasicIndex> result = new List<ArchiverBasicIndex>();
            result.AddRange(this.HeadAndBodyService.GetAllSubSites(info));
            logger.Info($"serch result that need return count is {result.Count}");
            return result;
        }
    }
}