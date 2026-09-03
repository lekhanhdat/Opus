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
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.FileSystem
{
    [Audit]
    public class FileSystemJobTimeReferenceService : RMServiceBase, IFileSystemJobTimeReferenceService
    {
        private RALogger logger = RALogger.GetInstance(typeof(FileSystemJobTimeReferenceService));

        public IFileSystemJobTimeReferenceDao fileSystemJobTimeReferenceDao => PlatformWindsorManager.GetService<IFileSystemJobTimeReferenceDao>();
        public DateTime GetLastJobTime(Guid scopeId)
        {
            var refrence = fileSystemJobTimeReferenceDao.GetJobEntry(scopeId);
            return refrence.LastJobTime;
        }

        public async Task<bool> UpdateJobTimeAsync(DateTime lastJobTime, string path, Guid scopeId)
        {
            try
            {
                logger.Debug("Start to update fs job time. Scope Id:" + scopeId);
                RMFileSystemJobTimeReference reference = new RMFileSystemJobTimeReference()
                {
                    LastJobTime = lastJobTime,
                    LastJobTimeTicks = lastJobTime.Ticks,
                    Path = path,
                    ScopeId = scopeId
                };
                await fileSystemJobTimeReferenceDao.AddOrUpdateAsync(reference);
                return true;
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while updating fs job time. Scope Id: {scopeId} Error: {e.ToString()}");
                return false;
            }
            finally
            {
                logger.Debug($"Update fs job time finished. Scope Id: {scopeId}");
            }
        }
    }
}
