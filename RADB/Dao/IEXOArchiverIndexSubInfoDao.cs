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
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao
{
    public interface IEXOArchiverIndexSubInfoDao: IBaseDao<EXOArchiverIndexSubInfo>
    {
        void CreateEXOSubInfo(EXOArchiverIndexSubInfo info);
        long GetArchiverStorageGBSize();
        Task<double> GetArchiverStorageGBSizeAsync(string storageId, CancellationToken cancellationToken = default);
        void UpdateEXOSubInfoSizeBySubSubJobId(string subSubJobId,long size);
        void UpdateEXOSubInfoMergeStatusBySubSubJobId(string subSubJobId, int status);
        EXOArchiverIndexSubInfo GetEXOArchiverSubInfoBySubSubJobId(string subSubJobId);
        Dictionary<string, double> GetAllEXOArchiverIndexSubInfoByMailboxAddresses(List<string> mailboxAddresses);
        Dictionary<string, double> GetAllEXOArchiverIndexSubInfoByMailboxAddresses(List<string> mailboxAddresses, long startTime, long endTime);
        List<EXOArchiverIndexSubInfo> GetAllArchiverIndexSubInfoByStorageId(string storageId);
        List<EXOArchiverIndexSubInfo> GetAllArchiverIndexSubInfo();
        Task<EXOArchiverIndexSubInfo> GetSubInfoBySubsubJobIdAsync(string subsubjobId);

        Dictionary<string, double> GetAllEXOArchivedSizeMapping((long, long)? archivedTimeRange = null);
        List<string> GetAllBackupOrMergeIndexFailedEXOSubJobIds();
        List<EXOArchiverIndexSubInfo> GetAllEXOArchiverIndexSubInfoByMainJobId(string mainJobId);
    }
}
