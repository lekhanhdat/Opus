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
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMLockDao : BaseDao<RMLock>, IRMLockDao
    {
        public RMLock GetLockerRecord(string tanantGroupId)
        {
            using (var context = GetNewContext())
            {
                RMLock lockObj = context.RMLock.AsQueryable().Where(p => p.TenantGroupId.Equals(tanantGroupId)).FirstOrDefault();
                return lockObj;
            }
        }

        public RMLock GetMaxLockerRecord(List<string> tenantGroupIds)
        {
            using var context = GetNewContext();
            var lockerRecords = context.RMLock.Where(l => tenantGroupIds.Contains(l.TenantGroupId));
            if (lockerRecords.Any())
            {
                return lockerRecords.OrderByDescending(l => l.RecordId).FirstOrDefault();
            }
            return null;
        }

        public RMLock GetLockerRecord(string tanantGroupId, out DateTime timeStamp, out int status, out long currentId, out byte[] rowVersion)
        {
            using (var context = GetNewContext())
            {
                timeStamp = DateTime.MinValue;
                status = -1;
                currentId = 0;
                rowVersion = null;
                RMLock lockObj = context.RMLock.AsQueryable().Where(p => p.TenantGroupId.Equals(tanantGroupId)).FirstOrDefault();
                if (lockObj != null)
                {
                    status = lockObj.Status;
                    currentId = lockObj.RecordId;
                    rowVersion = lockObj.RowVersion;
                    timeStamp = lockObj.UpdateTime;
                }
                return lockObj;
            }
        }

        public RMLock GetTimeStamp(string tanantGroupId)
        {
            //throw new NotImplementedException();
            using (var context = GetNewContext())
            {
                DateTime timeStamp = DateTime.MinValue;
                return context.RMLock.AsQueryable().Where(p => p.TenantGroupId.Equals(tanantGroupId)).FirstOrDefault();
            }
        }

        public void InserLockerRecord(RMLock lockObj)
        {
            using (var context = GetNewContext())
            {
                context.RMLock.Add(lockObj);
                context.SaveChanges();
            }
        }

        public async Task ReleaseLockerRecordAsync(RMLock lockObj)
        {
            await this.UpdateAsync(lockObj);
            //throw new NotImplementedException();
        }

        public async Task<bool> UpdateLockerRecordAsync(RMLock lockObj)
        {
            try
            {
                await this.UpdateAsync(lockObj);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
            //throw new NotImplementedException();
        }
    }
}
