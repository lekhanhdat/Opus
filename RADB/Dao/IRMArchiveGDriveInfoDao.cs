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
using AvePoint.RA.DB.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao
{
    public interface IRMArchiveGDriveInfoDao : IBaseDao<RMArchiveGDriveInfo>
    {
        Task<List<RMArchiveGDriveInfo>> GetGoogleArchiverByPagerAsync(int pageIndex, int pageSize, string searchKey = null);
        Task<int> GetGoogleArchiverTotalCount4DashboardAsync(string searchKey = null);
        Task<double> GetGoogleArchivedFileCount4DashboardAsync();
        Task<long> GetGoogleDeletedFileCount4DashboardAsync();
        void UpdateGoogleArchiverInfo(string driveName, long fileNumber, long versionNumber, string tenantId, string driveId, double size = 0);
        Task<int> SaveRetentionDriveInfo(RMRetentionGDriveInfo info);
        Task<int> DeleteAllAsync();
        void CreateOrUpdateDeletedInfo(string driveName, long size, string driveId, string tenantId, long deletedNumber);
        void UpdateGoogleArchiveInfo(string driveId, double size);

    }
}
