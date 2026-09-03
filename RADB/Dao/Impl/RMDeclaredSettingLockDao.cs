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

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMDeclaredSettingLockDao : BaseDao<RMDeclaredSettingLock>, IRMDeclaredSettingLockDao
    {
        public RMDeclaredSettingLock GetLockerRecord(string objectName)
        {
            using (var context = GetNewContext())
            {
                RMDeclaredSettingLock lockObj = context.RMDeclaredSettingLock.AsQueryable().Where(r=>r.ObjectName.Equals(objectName)).FirstOrDefault();
                return lockObj;
            }
        }

        public RMDeclaredSettingLock GetLockerRecord(string objectName, out DateTime timeStamp, out int status, out byte[] rowVersion)
        {
            using (var context = GetNewContext())
            {
                timeStamp = DateTime.MinValue;
                status = -1;
                rowVersion = null;
                RMDeclaredSettingLock lockObj = context.RMDeclaredSettingLock.AsQueryable().Where(r=>r.ObjectName.Equals(objectName)).FirstOrDefault();
                if (lockObj != null)
                {
                    status = lockObj.Status;
                    rowVersion = lockObj.RowVersion;
                    timeStamp = lockObj.UpdateTime;
                }
                return lockObj;
            }
        }

        public void InserLockerRecord(RMDeclaredSettingLock lockObj)
        {
            using (var context = GetNewContext())
            {
                context.RMDeclaredSettingLock.Add(lockObj);
                context.SaveChanges();
            }
        }

        public async Task ReleaseLockerRecordAsync(RMDeclaredSettingLock lockObj)
        {
            await this.UpdateAsync(lockObj);
        }

        public async Task<bool> UpdateLockerRecordAsync(RMDeclaredSettingLock lockObj)
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
        }
    }
}
