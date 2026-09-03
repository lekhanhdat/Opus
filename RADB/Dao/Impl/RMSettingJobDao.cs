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
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMSettingJobDao : BaseDao<RMSettingJobInfo>, IRMSettingJobDao
    {
        public bool AddRMSettingJob(RMSettingJobInfo info)
        {
            try
            {
                using (var context = this.GetNewContext())
                {
                    context.SettingJobInfo.Add(info);
                    context.SaveChanges();
                    return true;
                }
            }
            catch
            {
                return false;
            }
              //  throw new NotImplementedException();
        }
       

        public void DeleteRMSettingJob(string jobId)
        {
            using (var context = this.GetNewContext())
            {
                var settingJob = context.SettingJobInfo.AsQueryable().Where(s => s.Id.Equals(jobId, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (settingJob != null)
                {
                    context.SettingJobInfo.Remove(settingJob);
                    context.SaveChanges();
                }
            }
            // throw new NotImplementedException();
        }

        public RMSettingJobInfo GetRMSettingJob(string jobId)
        {
            using var context = GetNewContext();
            var settingJob = context.SettingJobInfo.AsQueryable().Where(s => s.Id.Equals(jobId, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            return settingJob;
        }

        public RMSettingJobInfo GetRMSettingJob(Expression<Func<RMSettingJobInfo, bool>> lamda)
        {
            using var context = GetNewContext();
            var settingJob = context.SettingJobInfo.AsQueryable().Where(lamda).FirstOrDefault();
            return settingJob;
        }

    }
}
