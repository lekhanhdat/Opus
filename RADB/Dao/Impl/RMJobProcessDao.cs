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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.Core;
using System.Data.Entity;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMJobProcessDao : BaseDao<RMJobProcess>,  IRMJobProcessDao
    {
        public string CreateJob(RMJobProcess job)
        {
            using (var ctx = RMDBContextManager.GetSystemDBContext())
            {
                if (ctx.JobProcess.Any(j => j.JobId.Equals(job.JobId) && j.TenantId.Equals(job.TenantId)))
                {
                    var tempJob = ctx.JobProcess.Where(j => j.JobId.Equals(job.JobId) && j.TenantId.Equals(job.TenantId)).FirstOrDefault();
                    tempJob.Status = job.Status;
                    tempJob.Progress = job.Progress;
                    tempJob.ModifiedTime = DateTime.UtcNow.Ticks;
                    tempJob.Comment = job.Comment;
                    this.Update(ctx, tempJob);
                }
                else
                {
                    ctx.JobProcess.Add(job);
                    ctx.SaveChanges();
                }
                return job.JobId;
            }
        }

        public RMJobProcess GetJobById(string jobId)
        {
            using (var ctx = RMDBContextManager.GetSystemDBContext())
            {
                return ctx.JobProcess.Where(j => j.JobId.Equals(jobId)).FirstOrDefault();
            }
        }

        public void UpdateJob(RMJobProcess job)
        {
            using (var ctx = RMDBContextManager.GetSystemDBContext())
            {
                if (ctx.JobProcess.Any(j => j.JobId.Equals(job.JobId) && j.TenantId.Equals(job.TenantId)))
                {
                    var tempJob = ctx.JobProcess.Where(j => j.JobId.Equals(job.JobId) && j.TenantId.Equals(job.TenantId)).FirstOrDefault();
                    tempJob.Status = job.Status;
                    tempJob.Progress = job.Progress;
                    tempJob.ModifiedTime = DateTime.UtcNow.Ticks;
                    tempJob.Comment = job.Comment;
                    this.Update(ctx, tempJob);
                }
            }
        }


        private bool Update(RMSysDBContext ctx, RMJobProcess entity)
        {
           
            var entry = ctx.Entry(entity);
            if (entry.State == EntityState.Modified)
            {
                return ctx.SaveChanges() > 0;
            }
            else if (entry.State == EntityState.Detached)
            {
                ctx.DetachLocalObject<RMJobProcess>(entity);
                ctx.Set<RMJobProcess>().Attach(entity);
                entry.State = EntityState.Modified;
                return ctx.SaveChanges() > 0;
            }
            return false;
            

        }
    }
}
