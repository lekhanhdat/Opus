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
    using AvePoint.GCommon.Contract.CommonFilter;
    using AvePoint.Media.Service.DomainModel;
    using global::Media.Service.DomainModel;

    #endregion using directives

    public interface IArchiverAdvancedSearchIndexService
    {
        List<ArchiverBasicIndex> Search(ArchiverRestoreFilter filter, ArchiverBrowseInfo restoreParam, ArchiverRestoreOrderBy orderBy, out long totalCount);
        List<ArchiverBasicIndex> SearchForFS(ArchiverRestoreFilter filter, ArchiverBrowseInfo restoreParam, ArchiverRestoreOrderBy orderBy);
        List<ArchiverBasicIndex> SearchForJob(ArchiverBrowseInfo restoreParam);
        DashBoardInfo SearchForJobV2();
        long SearchArchivedSizeForExportSubSite(string subSiteUrl, ArchiverBrowseInfo info);
        IAsyncEnumerable<ArchiverBasicIndex> SearchForExportAllItemAsync();
        IAsyncEnumerable<ArchiverBasicIndex> SearchForExportAllItemOnSpecificTimeRangeAsync(ArchiverBrowseInfo info);
        ArchiverBasicIndex GetParentFolder(ArchiverBasicIndex index);
        List<ArchiverBasicIndex> SearchSubsitesForExportAllSubSites(ArchiverBrowseInfo info);
        ArchiverBasicIndex GetItem(string path, long endTime);
    }
}