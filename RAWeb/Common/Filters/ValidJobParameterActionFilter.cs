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
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.DB.SecurityTrimming.Model;
using AvePoint.RA.Service.Services.AccountManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Common.Filters
{
    public class ValidJobParameterActionFilter : BaseActionFilter
    {
        private RALogger logger = RALogger.GetInstance(typeof(ValidJobParameterActionFilter));

        public ValidJobParameterActionFilter()
        {
        }

        protected override async Task OnActionAuthenticatedAsync(ActionExecutingContext actionContext)
        {
            var jobId = actionContext.ActionArguments.Values.FirstOrDefault()?.ToString();

            if (actionContext.ActionArguments.Values.FirstOrDefault() is JMDetailsQuery)
            {
                var jobDetailParamter = (JMDetailsQuery)actionContext.ActionArguments.Values.FirstOrDefault();
                jobId = jobDetailParamter?.JobID;
            }

            if (string.IsNullOrEmpty(jobId))
            {
                try
                {
                    var request_form_jobId = actionContext.HttpContext?.Request?.Form["jobIdString"];
                    if (!string.IsNullOrEmpty(request_form_jobId))
                    {
                        jobId = request_form_jobId.ToString();
                    }
                }
                catch (Exception ex)
                {
                    logger.Error($"An error while get job id from request form, message: {ex}");
                }
            }
            if (await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.JobMonitorAdmin) ||
                await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMSOPermissionMasks.JobMonitorAdmin) ||
                await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMDiscoveryPermissionMasks.AccessAll) ||
                await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMDiscoverySalesforcePermissionMask.AccessAll) ||
                await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMDiscoveryGoogleROTPermissionMask.AccessAll) ||
                await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMDiscoveryFileSystemPermissionMask.AccessAll) ||
                await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.JobMonitorEnduser) ||
                await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMSOPermissionMasks.JobMonitorEnduser))
            {
                return;
            }

            var jobIdList = new List<string>();
            if (actionContext.ActionArguments.Values.FirstOrDefault() is List<string>)
            {
                jobIdList = actionContext.ActionArguments.Values.FirstOrDefault() as List<string>;
            }
            else
            {
                jobIdList.Add(jobId);
            }


            ValidJobUtil util = new ValidJobUtil(jobIdList);
            if (!(await util.ValidateAsync()))
            {
                if(await util.ValidateRestoreJobsAsync())
                {
                    return;
                }
                actionContext.Result = new ObjectResult("Invalid parameter.") { StatusCode = (int)HttpStatusCode.Forbidden };
            }
            
        }
    }

    public class ValidMultipleJobParameterActionFilter : BaseActionFilter
    {
        public ValidMultipleJobParameterActionFilter()
        {
        }
        protected override async Task OnActionAuthenticatedAsync(ActionExecutingContext actionContext)
        {
            var jobIds = actionContext.ActionArguments.Values.FirstOrDefault() as List<string>;
            if (await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.JobMonitorAdmin) ||
                await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMSOPermissionMasks.JobMonitorAdmin) ||
                await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMDiscoveryPermissionMasks.AccessAll) ||
                await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMDiscoverySalesforcePermissionMask.AccessAll) ||
                await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMDiscoveryGoogleROTPermissionMask.AccessAll) ||
                await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMDiscoveryFileSystemPermissionMask.AccessAll)
)
            {
                return;
            }
            
            ValidJobUtil util = new ValidJobUtil(jobIds);
            if (!(await util.ValidateAsync()))
            {
                actionContext.Result = new ObjectResult("Invalid parameter.") { StatusCode = (int)HttpStatusCode.Forbidden };
            }
            
        }
    }

    public class ValidEnforceRuleActionJobParameterFilter : BaseActionFilter
    {
        public IArchiverJobDao mArhciverDao { get; set; }
        protected IArchiverJobDao ArhciverDao
        {
            get
            {
                if (mArhciverDao == null)
                {
                    mArhciverDao = (IArchiverJobDao)PlatformWindsorManager.GetService(typeof(IArchiverJobDao));
                }
                return mArhciverDao;
            }
        }
        private IJobMonitorDao mJMDao;
        protected IJobMonitorDao JMDao
        {
            get
            {
                if (mJMDao == null)
                {
                    mJMDao = (IJobMonitorDao)PlatformWindsorManager.GetService(typeof(IJobMonitorDao));
                }
                return mJMDao;
            }
        }
        public ValidEnforceRuleActionJobParameterFilter()
        {
        }
        protected override async Task OnActionAuthenticatedAsync(ActionExecutingContext actionContext)
        {
            var param = actionContext.ActionArguments.Values.FirstOrDefault() as JMDetailsQuery;
            if (param == null)
            {
                actionContext.Result = new ObjectResult("Invalid parameter.") { StatusCode = (int)HttpStatusCode.Forbidden };
            }
            else
            {
                if (!(await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.JobMonitorAdmin) || await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMSOPermissionMasks.JobMonitorAdmin)))
                {
                    var archiverJob = ArhciverDao.GetJobByID(param.JobID);
                    if (archiverJob != null)
                    {
                        var recoJob = JMDao.GetJob(archiverJob.RECOJobId);
                        if (recoJob != null)
                        {
                            ValidJobUtil util = new ValidJobUtil(new List<string>() { recoJob.Id });
                            if (!(await util.ValidateAsync()))
                            {
                                actionContext.Result = new ObjectResult("Invalid parameter.") { StatusCode = (int)HttpStatusCode.Forbidden };
                            }
                        }
                        else
                        {
                            actionContext.Result = new ObjectResult("Invalid records jobid.") { StatusCode = (int)HttpStatusCode.Forbidden };
                        }
                    }
                    else
                    {
                        actionContext.Result = new ObjectResult("Invalid archiver jobid.") { StatusCode = (int)HttpStatusCode.Forbidden };
                    }
                }
            }
        }
    }

    public class ValidJobUtil
    {
        private RALogger logger = RALogger.GetInstance(typeof(ValidJobUtil));
        private IJobMonitorDao mJMDao;
        protected IJobMonitorDao JMDao
        {
            get
            {
                if (mJMDao == null)
                {
                    mJMDao = (IJobMonitorDao)PlatformWindsorManager.GetService(typeof(IJobMonitorDao));
                }
                return mJMDao;
            }
        }
        private RMScopeRoleAssignmentDao RMScopeRoleAssignmentDao = new RMScopeRoleAssignmentDao();
        private IUserService mUserService = new UserService();
        public IRMSecurityTrimmingHelper SecurityTrimmingHelper
        {
            get
            {
                return (IRMSecurityTrimmingHelper)PlatformWindsorManager.GetService(typeof(IRMSecurityTrimmingHelper));
            }
        }
        private List<RMJobMonitor> mJobs;

        public ValidJobUtil(List<string> idArray)
        {
            mJobs = JMDao.GetJobs(idArray);
        }

        public async Task<bool> ValidateAsync()
        {
            if (HasInvalidJobTypes())
            {
                logger.Warn("Invalid job type.");
                return false;
            }

            if (!(await ValidateContainerIdJobsAsync()))
            {
                return false;
            }

            if (!(ValidateReportJobs()))
            {
                return false;
            }

            if (!(await ValidatePhysicalJobsAsync()))
            {
                return false;
            }

            if (!(await ValidateFSJobsAsync()))
            {
                return false;
            }

            if (!(await ValidateAzureFileShareJobsAsync()))
            {
                return false;
            }

            if (!(await ValidateBoxJobsAsync()))
            {
                return false;
            }

            if (!(await ValidateGoogleJobsAsync()))
            {
                return false;
            }

            if (!(await ValidateSPOnPremJobsAsync()))
            {
                return false;
            }

            return true;
        }

        public async Task<bool> ValidateRestoreJobsAsync()
        {
            if(await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMSOPermissionMasks.RestoreCenterSearch) 
               && mJobs.All(job => JobTypeConstants.RestoreOnlyPermissionJobTypes.Contains(job.JobType)))
            {
                return true;
            }

            return false;
        }

        private async Task<bool> ValidateContainerIdJobsAsync()
        {
            //非super admin，操作的job中包含container id是空的job（老数据），返回false
            var invalidJobs = mJobs.Where(j => JobTypeConstants.WithContainerIdJobTypes.Contains(j.JobType) && string.IsNullOrWhiteSpace(j.ContainerId)).Select(j => j.ContainerId).ToList();
            if (invalidJobs.Count > 0)
            {
                logger.Warn("Include invalid old jobs.");
                return false;
            }
            //获取container id集合，判断是否有权限
            var containerIds = mJobs.Where(j => JobTypeConstants.WithContainerIdJobTypes.Contains(j.JobType) && !string.IsNullOrWhiteSpace(j.ContainerId)).Select(j => j.ContainerId).Distinct().ToList();
            if (containerIds.Count > 0)
            {
                //physical disposal container id is Guid.Empty
                if (containerIds.Contains(Guid.Empty.ToString()))
                {
                    if (!(await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.PhysicalAdmin)))
                    {
                        logger.Warn("Include containers without physical permission.");
                        return false;
                    }
                    var containersWithoutPhysical = containerIds.Where(c => c != Guid.Empty.ToString()).ToList();
                    if (containersWithoutPhysical != null && containersWithoutPhysical.Count > 0)
                    {
                        List<string> userAndGroupUserIds = await mUserService.GetUserAndGroupUserIdsAsync(TenantLocalValue.LogonUserId);
                        if (!RMScopeRoleAssignmentDao.ValidateContainerIdPermission(containersWithoutPhysical, userAndGroupUserIds))
                        {
                            logger.Warn("Include containers without permission.");
                            return false;
                        }
                    }
                }
                else
                {
                    List<string> userAndGroupUserIds = await mUserService.GetUserAndGroupUserIdsAsync(TenantLocalValue.LogonUserId);
                    if (!RMScopeRoleAssignmentDao.ValidateContainerIdPermission(containerIds, userAndGroupUserIds))
                    {
                        logger.Warn("Include containers without permission.");
                        return false;
                    }
                }
            }
            return true;
        }

        private bool ValidateReportJobs()
        {
            var reportJobTypes = new List<int>();
            reportJobTypes.AddRange(JobTypeConstants.SPReportTypes);
            reportJobTypes.AddRange(JobTypeConstants.EXOReportTypes);
            reportJobTypes.AddRange(JobTypeConstants.PhysicalReportTypes);
            reportJobTypes.AddRange(JobTypeConstants.FSReportTypes);
            reportJobTypes.AddRange(JobTypeConstants.SpecialJobTypes);
            reportJobTypes.AddRange(JobTypeConstants.OneDriveReportTypes);
            reportJobTypes.AddRange(JobTypeConstants.SPOnPremReportTypes);
            reportJobTypes.AddRange(JobTypeConstants.BoxReportTypes);
            reportJobTypes.AddRange(JobTypeConstants.GoogleReportTypes);
            reportJobTypes.AddRange(JobTypeConstants.TeamsReportTypes);
            var reportJobUserIds = mJobs.Where(j => reportJobTypes.Contains(j.JobType) && !string.IsNullOrWhiteSpace(j.ContainerId)).Select(j => j.ContainerId).Distinct().ToList();
            if (reportJobUserIds.Count > 0)
            {
                //如果有非logon user run的report job，则返回false
                var invalidJobs = reportJobUserIds.Where(i => !i.Equals(TenantLocalValue.LogonUserId, StringComparison.OrdinalIgnoreCase)).ToList();
                if (invalidJobs.Count > 0)
                {
                    logger.Warn("Include report jobs run by other user.");
                    return false;
                }
            }
            return true;
        }

        private async Task<bool> ValidatePhysicalJobsAsync()
        {
            var physicalJobs = mJobs.Where(j => JobTypeConstants.PhysicalJobTypes.Contains(j.JobType)).ToList();
            if (physicalJobs.Count > 0)
            {
                //如果有physical job但是当前user没有physical admin权限，则返回false
                if (!(await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.PhysicalAdmin)))
                {
                    logger.Warn("Invalid physical permission.");
                    return false;
                }
            }
            return true;
        }

        private async Task<bool> ValidateFSJobsAsync()
        {
            var fsJobs = mJobs.Where(j => JobTypeConstants.FSJobTypes.Contains(j.JobType)).ToList();
            if (fsJobs.Count > 0)
            {
                //如果有fs job但是当前user没有fs admin权限，则返回false
                if (!(await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.FSAdmin)))
                {
                    logger.Warn("Invalid fs permission.");
                    return false;
                }
            }
            return true;
        }

        private async Task<bool> ValidateAzureFileShareJobsAsync()
        {
            var jobs = mJobs.Where(item => JobTypeConstants.AzureFileShareJobTypes.Contains(item.JobType));
            if (jobs.Any())
            {
                if (!(await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.AzureFSAdmin)))
                {
                    logger.Warn("Invalid azure file share permission.");
                    return false;
                }
            }

            return true;
        }

        private async Task<bool> ValidateBoxJobsAsync()
        {
            var jobs = mJobs.Where(item => JobTypeConstants.BoxJobTypes.Contains(item.JobType));
            if (jobs.Any())
            {
                if (!(await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.BoxAdmin)))
                {
                    logger.Warn("Invalid box permission.");
                    return false;
                }
            }

            return true;
        }

        private async Task<bool> ValidateGoogleJobsAsync()
        {
            var jobs = mJobs.Where(item => JobTypeConstants.GoogleJobTypes.Contains(item.JobType));
            if (jobs.Any())
            {
                if (!(await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.GoogleAdmin)))
                {
                    logger.Warn("Invalid google permission.");
                    return false;
                }
            }

            return true;
        }

        private async Task<bool> ValidateSPOnPremJobsAsync()
        {
            var onPremJobs = mJobs.Where(j => JobTypeConstants.SPOnPremJobTypes.Contains(j.JobType)).ToList();
            if (onPremJobs.Count > 0)
            {
                //如果有sp on premise job但是当前user没有sp on premise enduser权限，则返回false
                if (!(await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.SPOnPremEnduser)))
                {
                    logger.Warn("Invalid sp on premise permission.");
                    return false;
                }
            }
            return true;
        }

        private bool HasInvalidJobTypes()
        {
            List<int> validJobTypes = new List<int>();
            validJobTypes.AddRange(JobTypeConstants.WithContainerIdJobTypes);
            validJobTypes.AddRange(JobTypeConstants.EXOReportTypes);
            validJobTypes.AddRange(JobTypeConstants.SPReportTypes);
            validJobTypes.AddRange(JobTypeConstants.PhysicalReportTypes);
            validJobTypes.AddRange(JobTypeConstants.FSReportTypes);
            validJobTypes.AddRange(JobTypeConstants.BoxReportTypes);
            validJobTypes.AddRange(JobTypeConstants.FSJobTypes);
            validJobTypes.AddRange(JobTypeConstants.PhysicalJobTypes);
            validJobTypes.AddRange(JobTypeConstants.SpecialJobTypes);
            validJobTypes.AddRange(JobTypeConstants.SPOnPremJobTypes);
            validJobTypes.AddRange(JobTypeConstants.SPOnPremReportTypes);
            validJobTypes.AddRange(JobTypeConstants.OneDriveReportTypes);
            validJobTypes.AddRange(JobTypeConstants.AzureFileShareJobTypes);
            validJobTypes.AddRange(JobTypeConstants.BoxJobTypes);
            validJobTypes.AddRange(JobTypeConstants.ArchiverJobTypes);
            validJobTypes.AddRange(JobTypeConstants.GoogleReportTypes);
            validJobTypes.AddRange(JobTypeConstants.GoogleJobTypes);
            validJobTypes.AddRange(JobTypeConstants.TeamsJobTypes);
            validJobTypes.AddRange(JobTypeConstants.TeamsReportTypes);

            var invalidJobs = mJobs.Where(j => !validJobTypes.Contains(j.JobType)).ToList();
            return invalidJobs.Count > 0;
        }

    }
}