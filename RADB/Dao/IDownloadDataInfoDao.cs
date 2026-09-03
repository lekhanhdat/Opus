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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao
{
    public interface IDownloadDataInfoDao: IBaseDao<RMDownloadDataInfo>
    {
        bool CreateDownloadDataInfo(RMDownloadDataInfo  downloadDataInfo);
        bool CreateZipPasswordInfo(RMDownloadDataInfo downloadDataInfo);

        void BatchDeleteDownloadDataInfoByIds(List<Guid> ids);

        List<RMDownloadDataInfo> QueryDownloadDataInfoById(string searchKey, int pageIndex, int pageSize, out int totalRecord);
        List<RMDownloadDataInfo> QueryDownloadReportInfoByScopeIds(List<string> scopeIds, int jobType, int pageIndex, int pageSize, out int totalRecord, string orderBy = null, bool isDesc = false);
        List<RMDownloadDataInfo> QueryAllDownloadReportInfo(int jobType, int pageIndex, int pageSize, out int totalRecord, string orderBy = null, bool isDesc = false);
        bool IsHasInprogressRCCReport(List<string> jobIds);
        List<RMDownloadDataInfo> GetDownloadDataInfoByRetentionTime(long retentionTime);
        List<RMDownloadDataInfo> GetZipPasswordInfoByRetentionTime(long retentionTime);

        bool ExistAvailableJob(Guid recordsId);

        List<RMDownloadDataInfo> GetDownloadDataInfos(List<Guid> ids, List<int> status = null);

        List<RMDownloadDataInfo> GetDownloadDataInfos(List<string> jobIds, List<int> status = null);

        List<RMDownloadDataInfo> GetDownloadDataInfosByStatus(List<int> status);
        RMDownloadDataInfo GetDownloadDataInfosByJobId(string jobId);

        bool ApplyCurrentValues(RMDownloadDataInfo downloadDataInfo);

        bool UpdateDownloadInfo(RMDownloadDataInfo downloadDataInfo);

        bool UpdateDownloadFileSizeByJobId(string jobId, long fileSize);

        string GetBlobSasUriByJobId(string jobId);

        string GetBlobSasUriByRecordId(Guid recordId);

        bool UpdateBlobSasUriByJobId(string jobId, string blobSasUri);

        long? GetDownloadFileSizeByJobId(string jobId);

        RMDownloadDataInfo GetDownloadDataInfoByJobId(string jobId);
    }
}
