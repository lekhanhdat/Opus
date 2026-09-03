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
    using System.Text;
    using AvePoint.GCommon.Contract.CommonFilter;
    using AvePoint.Media.Core.Index;
    using AvePoint.Media.Service.DomainModel;
    using AvePoint.RA.CommonUtil;

    #endregion using directives

    public class ArchiverAdvancedSearchIndexService
        : ArchiverIndexServiceBase
        , IArchiverAdvancedSearchIndexService
    {
        static readonly String selectSection = "SELECT MAX(COL_ARCHIVE_TIME) AS COL_ARCHIVE_TIME, COL_EXTENSION_7, COL_TYPE, COL_NAME, COL_PATH_MD5, COL_PARENT_PATH_MD5, COL_ATTRIBUTES, COL_JOBID, COL_CREATE_TIME, COL_MODIFY_TIME,COL_SITE_PATH,COL_EXTENSION_9,COL_EXTENSION_5,COL_EXTRAINFO,COL_EXTENSION_2,COL_RETENTION_STATUS,count(*) over() as COL_PLATFORM_TYPE";
        static readonly String selectSectionForJob = "SELECT COL_TYPE, COL_NAME from TB_BODY_INDEX where (COL_TYPE = 'D' or COL_TYPE = 'V')";
        private RALogger logger = RALogger.GetInstance(typeof(ArchiverAdvancedSearchIndexService));
        //public ISqlBuilder _SqlBuilder { get; set; }
        //public ISqlBuilder SqlBuilder
        //{
        //    get
        //    {
        //        if (_SqlBuilder == null)
        //        {
        //            _SqlBuilder = new ArchiverSqlBuilder();
        //            return _SqlBuilder;
        //        }
        //        else
        //        {
        //            return _SqlBuilder;
        //        }
        //    }
        //    set { }
        //}
        public ArchiverBasicIndex GetParentFolder(ArchiverBasicIndex index)
        {
            return this.HeadAndBodyService.GetParentDataFromHeadTable(index);
        }

        public ArchiverBasicIndex GetItem(string path, long endTime)
        {
            return this.HeadAndBodyService.GetOneDataFromHeadOrBodyTable(path, endTime);
        }

        //public List<ArchiverBasicIndex> Search(ArchiverRestoreFilter filter, ArchiverBrowseInfo restoreParam)
        //{
        //    var result = new List<ArchiverBasicIndex>();
        //    var tempList = new List<ArchiverBasicIndex>();
        //    var sql = new StringBuilder();
        //    var parameters = new Dictionary<String, Object>();
        //    var buildInfo = new SqlBuildInfo(selectSection, filter);
        //    sql = SqlBuilder.Build(buildInfo);
        //    result = this.HeadAndBodyService.GetAllDatasFromHeadOrBodyTableByType(sql, filter, restoreParam);
        //    logger.Info($"serch result that need return count is {result.Count}");
        //    return result;
        //}

        //public List<ArchiverBasicIndex> SearchForJob(ArchiverBrowseInfo restoreParam)
        //{
        //    var result = new List<ArchiverBasicIndex>();
        //    var tempList = new List<ArchiverBasicIndex>();
        //    var sql = new StringBuilder();
        //    var parameters = new Dictionary<String, Object>();
        //    var buildInfo = new SqlBuildInfo(selectSectionForJob, new ArchiverRestoreFilter() { Level = PolicyLevel.Document});
        //    //sql = SqlBuilder.Build(buildInfo);
        //    result = this.HeadAndBodyService.GetAllDatasFromHeadOrBodyTableByTypeForJob(selectSectionForJob, restoreParam);
        //    logger.Info($"serch result that need return count is {result.Count}");
        //    return result;
        //}

        public long SearchArchivedSizeForExportSubSite(string subSiteUrl, ArchiverBrowseInfo info)
        {
            return this.HeadAndBodyService.GetSubSiteArchiveSize(subSiteUrl, info);
        }

        public List<ArchiverBasicIndex> SearchForExportAllItem()
        {
            List<ArchiverBasicIndex> result = new List<ArchiverBasicIndex>();
            result.AddRange(this.HeadAndBodyService.GetAllHeadIndex());
            result.AddRange(this.HeadAndBodyService.GetAllBodyIndex());
            logger.Info($"serch result that need return count is {result.Count}");
            return result;
        }

        public List<ArchiverBasicIndex> SearchForExportAllItemOnSpecificTimeRange(ArchiverBrowseInfo info)
        {
            List<ArchiverBasicIndex> result = new List<ArchiverBasicIndex>();
            result.AddRange(this.HeadAndBodyService.GetAllHeadIndexOnSpecificTimeRange(info));
            result.AddRange(this.HeadAndBodyService.GetAllBodyIndexOnSpecificTimeRange(info));
            logger.Info($"serch result that need return count is {result.Count}");
            return result;
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