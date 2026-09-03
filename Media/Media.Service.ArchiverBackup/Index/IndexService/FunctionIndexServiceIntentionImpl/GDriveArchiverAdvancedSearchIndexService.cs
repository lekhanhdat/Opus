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
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.Media.Service.ArchiverBackup;
using AvePoint.Media.Service.DomainModel;
using AvePoint.RA.CommonUtil;
using Media.Service.ArchiverBackup.Index.IndexService.FunctionIndexServiceIntention;
using Media.Service.DomainModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Media.Service.ArchiverBackup.Index.IndexService.FunctionIndexServiceIntentionImpl
{
    public class GDriveArchiverAdvancedSearchIndexService : GDriveArchiverIndexServiceBase, IGDriveArchiverAdvancedSearchIndexService
    {
        static readonly String selectSection = "SELECT COL_ARCHIVE_TIME, COL_PATH, COL_ITEMID, COL_DRIVE_ID, COL_TYPE, COL_NAME, COL_PATH_MD5, COL_PARENT_PATH_MD5, COL_ATTRIBUTES, COL_CREATE_TIME, COL_MODIFY_TIME, COL_CONTENT_LENGTH, COL_RETENTION_STATUS,count(*) over() as COL_PLATFORM_TYPE";

        private RALogger _logger = RALogger.GetInstance(typeof(GDriveArchiverAdvancedSearchIndexService));
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

        public GoogleBasicIndex GetParentFolder(GoogleBasicIndex index)
        {
            return this.HeadAndBodyService.GetParentDataFromHeadTable(index);
        }

        public List<GoogleBasicIndex> SearchForExportAllItem()
        {
            var result = new List<GoogleBasicIndex>();
            result.AddRange(this.HeadAndBodyService.GetAllHeadIndex());
            result.AddRange(this.HeadAndBodyService.GetAllBodyIndex());
            _logger.Info($"search result that need return count is {result.Count}");
            return result;
        }

        public List<GoogleBasicIndex> SearchForExportAllItemOnSpecificTimeRange(GDriveBrowseInfo info)
        {
            var result = new List<GoogleBasicIndex>();
            result.AddRange(this.HeadAndBodyService.GetAllHeadIndexOnSpecificTimeRange(info));
            result.AddRange(this.HeadAndBodyService.GetAllBodyIndexOnSpecificTimeRange(info));
            _logger.Info($"search result that need return count is {result.Count}");
            return result;
        }

        public List<GoogleBasicIndex> SearchForGoogle(ArchiverRestoreFilter filter, GDriveBrowseInfo restoreParam, ArchiverRestoreOrderBy orderBy)
        {
            var result = new List<GoogleBasicIndex>();
            var sql = new StringBuilder();
            var buildInfo = new SqlBuildInfo(selectSection, filter);
            sql = SqlBuilder.Build(buildInfo);
            result = this.HeadAndBodyService.GetAllGoogleDatasFromItemTableByType(sql, filter, restoreParam, orderBy);
            _logger.Info($"serch google result that need return count is {result.Count}");
            return result;
        }

        public DashBoardInfo SearchForJob()
        {
            DashBoardInfo dashBoardInfo = new DashBoardInfo();
            dashBoardInfo.DocumentCount = this.HeadAndBodyService.GetFileCount();
            dashBoardInfo.VersionCount = this.HeadAndBodyService.GetFileVersionCount();
            dashBoardInfo.VersionNumber = (double)dashBoardInfo.VersionCount / 1000;
            dashBoardInfo.FileNumber = (double)dashBoardInfo.DocumentCount / 1000;
            _logger.Info($"SearchForJob.DocumentCount:{dashBoardInfo.DocumentCount}.VersionCount:{dashBoardInfo.VersionCount}.VersionNumber:{dashBoardInfo.VersionNumber}.FileNumber:{dashBoardInfo.FileNumber}.");
            return dashBoardInfo;
        }
    }
}
