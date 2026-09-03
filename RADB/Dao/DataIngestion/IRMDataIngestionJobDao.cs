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
using AvePoint.RA.Contract.DataIngestion;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Model.DataIngestion;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.DataIngestion
{
    public interface IRMDataIngestionJobDao
    {
        Task<RMDataIngestionJob> GetByIdAsync(string id);

        Task<bool> IsJobRunning(RMDataIngestionType type);

        Task<List<RMDataIngestionJob>> GetByStatusAsync(params JobStatus[] status);

        Task AddOrUpdateAsync(RMDataIngestionJob job);

        Task AddOrUpdateAsync(IEnumerable<RMDataIngestionJob> jobs);

        Task<bool> UpdateStatusAsync(string id, JobStatus status, long modifiedTime);

        Task<bool> DeleteAsync(string id);

        Task<int> UpdateStatusToTimeoutAsync(long modifiedTimeUpperBound);

        Task<bool> UpdateModifiedTimeAsync(string id, long modifiedTime);
        Task<RMDataIngestionJob> GetExistingJobByUniqueId(string uniqueId);

    }
}
