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
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.CodeView;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class ProfileDao : BaseDao<RMProfile>, IProfileDao
    {
        public IJobMonitorDao JobDao { get; set; }

        /// <summary>
        /// 并不在数据库中真正删除，只是做个标记
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [RACodeReview("Allen Yin")]
        public bool DeleteProfiles(List<int> ids)
        {
            using var context = GetNewContext();
            if (ids != null && ids.Count > 0)
            {
                var profiles = context.Profile.AsQueryable().Where(p => ids.Contains(p.Id)).ToList();
                foreach (var profile in profiles)
                {
                    profile.IsRemoved = true;
                }
                this.BatchUpdate(profiles, p => p.IsRemoved);
            }
            return false;
        }

        /// <summary>
        /// 真正删除，只有当job也删除了，即再也无法看report了之后，才调用此方法完全删除Profile
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        //[RACodeReview("Allen Yin")]
        //public bool RealDeleteProfiles(List<int> ids)
        //{
        //    if (ids != null && ids.Count > 0)
        //    {
        //        return this.BatchDelete(a => ids.Contains(a.Id)) == ids.Count;
        //    }
        //    return false;
        //}

        public async Task<bool> RealDeleteProfilesAndJobsAsync(List<int> ids)
        {
            if(ids != null && ids.Count > 0)
            {
                List<int?> tempIds = new List<int?>();
                foreach (var id in ids)
                {
                    tempIds.Add(id);
                }
                using var context = GetNewContext();
                List<RMJobMonitor> jobs = await JobDao.FindListAsync(jm => jm.ProfileId != null && tempIds.Contains(jm.ProfileId));
                foreach (RMJobMonitor jm in jobs)
                {
                    context.Set<RMJobMonitor>().Attach(jm);
                    context.Entry(jm).State = EntityState.Deleted;
                }

                List<RMProfile> profiles = await FindListAsync(p => ids.Contains(p.Id));
                foreach (RMProfile profile in profiles)
                {
                    context.Set<RMProfile>().Attach(profile);
                    context.Entry(profile).State = EntityState.Deleted;
                }
                return context.SaveChanges() > 0;
            }
            else
            {
                return false;
            }
        }

        [RACodeReview("Allen Yin")]
        public bool EditProfile(RMProfile profile)
        {
            try
            {
                using (var context = GetNewContext())
                {
                    profile.Modified = DateTime.UtcNow.Ticks;
                    return this.ApplyCurrentValues(context, profile);
                }
            }
            catch (DbEntityValidationException dbex)
            {
                string message = string.Join("; ", dbex.EntityValidationErrors.SelectMany(x => x.ValidationErrors).Select(x => x.ErrorMessage));
                throw new DbEntityValidationException(message);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [RACodeReview("Allen Yin", comment: "看来count()效率还可以")]
        public List<RMProfile> GetProfiles(int pageIndex, int pageSize, out int totalRecord, string orderKey, bool isAsc, Expression<Func<RMProfile, bool>> whereLambda = null)
        {
            try
            {
                using (var context = GetNewContext())
                {
                    IOrderedQueryable<RMProfile> query = null;
                    var sortDirection = isAsc ? SortDirectionEnum.Descending : SortDirectionEnum.Ascending;
                    if (whereLambda != null)
                    {
                        query = context.Profile.AsQueryable().Where(whereLambda).SortBy(orderKey, sortDirection);
                    }
                    else
                    {
                        query = context.Profile.AsQueryable().SortBy(orderKey, sortDirection);
                    }
                    totalRecord = query.Count();
                    return query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<RMProfile> GetJobNotificationProfiles()
        {
            using var context = GetNewContext();
            var result = context.Profile.AsQueryable().Where(p => p.Type == (int)JobType.JobNotification && !p.IsRemoved).ToList();
            return result;
        }

        public List<int> GetValidProfileTypesByUserId(string userId)
        {
            try
            {
                using (var context = GetNewContext())
                {
                    var types = context.Profile.AsQueryable().Where(p => p.CreateProfileLogonUserId == userId && p.IsRemoved == false).Select(p => p.Type).Distinct().ToList();
                    return types;
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }

        [RACodeReview("Allen Yin")]
        public int SaveProfile(RMProfile profile)
        {
            try
            {
                using (var context = GetNewContext())
                {
                    profile.Modified = DateTime.UtcNow.Ticks;
                    context.Profile.Add(profile);
                    context.SaveChanges(); 
                }
                return profile.Id;
            }
            catch (DbEntityValidationException dbex)
            {
                string message = string.Join("; ", dbex.EntityValidationErrors.SelectMany(x => x.ValidationErrors).Select(x => x.ErrorMessage));
                throw new DbEntityValidationException(message);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [RACodeReview("Allen Yin")]
        public RMProfile GetProfileById(int profileId)
        {
            //var context = SharedDbContext;
            using (var context = GetNewContext())
            {
                var profile = context.Profile.Find(profileId); //.AsQueryable().Where(sc => sc.Id.Equals(profileId)).FirstOrDefault();
                return profile; 
            }
        }
        public RMProfile GetProfileByScheduleId(string scheduleId)
        {
            //var context = SharedDbContext;
            using (var context = GetNewContext())
            {
                var profile = context.Profile.AsQueryable().Where(sc => sc.ScheduleId.Equals(scheduleId)).FirstOrDefault();
                return profile;
            }
        }
        [RACodeReview("Allen Yin")]
        public List<RMProfile> GetProfileByIds(List<int> ids)
        {
            //var context = SharedDbContext;
            using (var context = GetNewContext())
            {
                return context.Profile.AsQueryable().Where(sc => ids.Contains(sc.Id)).ToList(); 
            }
        }

        [RACodeReview("Allen Yin")]
        public IEnumerable<RMProfile> GetProfilesByTypes(List<JobType> jobTypes, List<SourceFlag> sources, string logonUserId = "")
        {
            using var context = GetNewContext();
            IEnumerable<RMProfile> profiles = null;
            var intJobTypes = jobTypes.Select(j => (int)j);
            if (string.IsNullOrEmpty(logonUserId))
            {
                profiles = context.Profile.AsQueryable().Where(p => intJobTypes.Contains(p.Type)).OrderByDescending(p => p.Modified).ToList();
            }
            else
            {
                profiles = context.Profile.AsQueryable().Where(p => p.CreateProfileLogonUserId == logonUserId && intJobTypes.Contains(p.Type) && sources.Contains(p.Source)).OrderByDescending(p => p.Modified).ToList();
            }
            return profiles;
        }

        [RACodeReview("Allen Yin")]
        public bool CheckProfileNameExist(RMProfileDto profile)
        {
            using var context = GetNewContext();
            var isTermUsage = JobTypeConstants.TermUsageJobTypes.Contains((int)profile.Type);
            bool exist = false;
            if (isTermUsage)
            {
                exist = context.Profile.AsQueryable().Any(sc => !sc.IsRemoved && JobTypeConstants.TermUsageJobTypes.Contains((int)sc.Type) && sc.Name.Equals(profile.ProfileName));
            }
            else
            {
                List<int> jobTypes = new List<int>();
                switch (profile.Type)
                {
                    case JobType.EXOItemsFilesDueDisposalReport:
                    case JobType.FSItemsFilesDueDisposal:
                    case JobType.ItemsFilesDueDisposal:
                    case JobType.OneDriveItemsFilesDueDisposalReport:
                    case JobType.PhysicalItemsFilesDueDisposalReport:
                    case JobType.SPOnPremItemsFilesDueDisposal:
                    case JobType.BoxItemsFilesDueDisposalReport:
                    case JobType.GoogleItemsFilesDueDisposalReport:
                    case JobType.TeamsItemsFilesDueDisposalReport:
                        jobTypes = JobTypeConstants.ContentDueReportJobTypes;
                        break;
                    case JobType.CreateAndDestroyedFileReport:
                    case JobType.EXOCreateAndDestroyedFileReport:
                    case JobType.FSCreateAndDestroyedFileReport:
                    case JobType.OneDriveCreateAndDestroyedFileReport:
                    case JobType.PhysicalCreateAndDestroyedFileReport:
                    case JobType.SPOnPremCreateAndDestroyedFileReport:
                    case JobType.BoxCreateAndDestroyedFileReport:
                    case JobType.GoogleCreateAndDestroyedFileReport:
                    case JobType.TeamsCreateAndDestroyedFileReport:
                        jobTypes = JobTypeConstants.CreationReportJobTypes;
                        break;
                    case JobType.AvailableSpaceReport:
                        jobTypes = JobTypeConstants.AvaliableSpaceReportJobTypes;
                        break;
                    case JobType.SPOActionAuditReport:
                    case JobType.OneDriveActionAuditReport:
                    case JobType.TeamsActionAuditReport:
                        jobTypes = JobTypeConstants.ActionAuditReportJobTypes;
                        break;
                    case JobType.RestoreReport:
                    case JobType.OneDriverRestoreReport:
                    case JobType.TeamsRestoreReport:
                        jobTypes = JobTypeConstants.RestoreReportJobTypes;
                        break;
                    case JobType.ArchivedSiteReport:
                    case JobType.OneDriveArchivedSiteReport:
                    case JobType.TeamsArchivedSiteReport:
                    case JobType.GoogleArchivedSiteReport:
                        jobTypes = JobTypeConstants.ArchivedSiteReportJobTypes;
                        break;
                    case JobType.JobNotification:
                        jobTypes = [(int)JobType.JobNotification];
                        break;
                    default:
                        break;
                }
                exist = context.Profile.AsQueryable().Any(sc => !sc.IsRemoved && jobTypes.Contains(sc.Type) && sc.Name.Equals(profile.ProfileName));
            }
            return exist;
        }

        /// <summary>
        /// 获取未generate report的profile在第几页
        /// </summary>
        /// <param name="profileId"></param>
        /// <returns></returns>
        public int GetPageIndexByProfileId(int profileId)
        {
            using var context = GetNewContext();
            var sortDirection = true ? SortDirectionEnum.Descending : SortDirectionEnum.Ascending;
            List<RMProfile> listAll = context.Profile.AsQueryable().SortBy("Modified", sortDirection).ToList();
            int totalRecord = listAll.Count;
            int pageSize = 15;
            int pageIndex = -1;
            for (int i = 0; i < totalRecord; i++)
            {
                if (listAll[i].Id == profileId)
                {
                    return i / pageSize + 1;
                }

            }
            return pageIndex;
        }

    }
}
