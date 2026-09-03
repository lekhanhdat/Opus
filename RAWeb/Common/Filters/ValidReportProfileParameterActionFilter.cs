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
using Aspose.Pdf.Operators;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.Service.Services.AccountManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using RATeams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Common.Filters
{
    public class ValidReportProfileParameterActionFilter : BaseActionFilter
    {
        private RALogger logger = RALogger.GetInstance(typeof(ValidReportProfileParameterActionFilter));
        protected IRMReportService RMReportService => PlatformWindsorManager.GetService<IRMReportService>();
        public ValidReportProfileParameterActionFilter()
        {
        }
        protected override async Task OnActionAuthenticatedAsync(ActionExecutingContext actionContext)
        {
            var dto = actionContext.ActionArguments.Values.First() as RMProfileDto;
            ValidReportUtil util = new ValidReportUtil();
            var realProfile = await RMReportService.GetProfileByIdAsync(dto.Id.ToString());
            RMProfileDto realtermprofile = RMReportService.GetProfileByIdForReportJob(dto.Id.ToString());
            if (realProfile.Type != dto.Type)
            {
                logger.Warn($"Invalid profile type:{dto.Type}");
                actionContext.Result = new ObjectResult("Invalid type") { StatusCode = (int)HttpStatusCode.Forbidden };
                return;
            }
            else
            {
                dto.Extension1 = realtermprofile.Extension1;
                dto.Extension2 = realProfile.Extension2;
            }
                
            bool isAdmin = await util.IsAdminAsync();
            bool isSOAdmin = await util.IsSOAdminAsync();
            if (!(isAdmin && await util.ValidatePermissionByJobTypeAsync((int)dto.Type, isAdmin))
                && !(isSOAdmin && await util.ValidateSOPermissionByJobTypeAsync((int)dto.Type, isSOAdmin)))
            {
                if (!(await util.ValidateTermAsync(dto)))
                {
                    logger.Warn($"Current user has no access on term.");
                    actionContext.Result = new ObjectResult("No access on term") { StatusCode = (int)HttpStatusCode.Forbidden };
                    return;
                }
                if (!realProfile.CreateProfileUserId.Equals(TenantLocalValue.LogonUserId, StringComparison.OrdinalIgnoreCase))
                {
                    logger.Warn($"Current user has no access for profile: {realProfile.ProfileName}");
                    actionContext.Result = new ObjectResult("Invalid user id") { StatusCode = (int)HttpStatusCode.Forbidden };
                    return;
                }
                if (!(await util.ValidatePermissionByJobTypeAsync((int)dto.Type, isAdmin))
                    && !(await util.ValidateSOPermissionByJobTypeAsync((int)dto.Type, isSOAdmin)))
                {
                    logger.Warn($"Current user has no permission for this type:{dto.Type}");
                    actionContext.Result = new ObjectResult("Invalid permission") { StatusCode = (int)HttpStatusCode.Forbidden };
                    return;
                }
            }
            if (!(await util.ValidateParameterAsync(dto, isAdmin, dto.Id == 0))
                && !(await util.ValidateSOParameterAsync(dto, isSOAdmin, dto.Id == 0)))
            {
                logger.Warn($"Current user has no access on container.");
                actionContext.Result = new ObjectResult("No access on containder") { StatusCode = (int)HttpStatusCode.Forbidden };
            }
        }
    }

    public class ValidCreateReportProfileParameterActionFilter : BaseActionFilter
    {
        private RALogger logger = RALogger.GetInstance(typeof(ValidCreateReportProfileParameterActionFilter));
        private IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private IRMReportService mReportService;
        protected IRMReportService RMReportService
        {
            get
            {
                if (mReportService == null)
                {
                    mReportService = (IRMReportService)PlatformWindsorManager.GetService(typeof(IRMReportService));
                }
                return mReportService;
            }
        }
        public ValidCreateReportProfileParameterActionFilter()
        {
        }

        private static readonly HashSet<JobType> JPMCJobTypes = new()
        {
            JobType.FSCreateAndDestroyedFileReport,
            JobType.FSItemsFilesDueDisposal,
            JobType.FSBCSTermUsageReport
        };
        protected override async Task OnActionAuthenticatedAsync(ActionExecutingContext actionContext)
        {
            var dto = actionContext.ActionArguments.Values.First() as RMProfileDto;
            if (JPMCJobTypes.Contains(dto.Type) && RMKeyValueDao.IsEnableJPMCFileSystemFeature())
            {
                logger.Warn($"This feature '{dto.Type}' is not available for JPMC");
                actionContext.Result = new ObjectResult("Invalid permission") { StatusCode = (int)HttpStatusCode.Forbidden };
                return;
            }

            ValidReportUtil util = new ValidReportUtil();
                
            bool isAdmin = await util.IsAdminAsync();
            bool isSOAdmin = await util.IsSOAdminAsync();
            if (!(isAdmin && await util.ValidatePermissionByJobTypeAsync((int)dto.Type, isAdmin))
                && !(isSOAdmin && await util.ValidateSOPermissionByJobTypeAsync((int)dto.Type, isSOAdmin)))
            {
                if (!(await util.ValidateTermAsync(dto)))
                {
                    logger.Warn($"Current user has no access on term.");
                    actionContext.Result = new ObjectResult("No access on term") { StatusCode = (int)HttpStatusCode.Forbidden };
                    return;
                }

                if (!(await util.ValidatePermissionByJobTypeAsync((int)dto.Type, isAdmin))
                    && !(await util.ValidateSOParameterAsync(dto, isSOAdmin, false)))
                {
                    logger.Warn($"Current user has no permission for this type:{dto.Type}");
                    actionContext.Result = new ObjectResult("Invalid permission") { StatusCode = (int)HttpStatusCode.Forbidden };
                    return;
                }
            }

            if (!(await util.ValidateParameterAsync(dto, isAdmin, JobTypeConstants.EXOReportTypes.Contains((int)dto.Type)))
                && !(await util.ValidateSOParameterAsync(dto, isSOAdmin, false)))
            {
                logger.Warn($"Current user has no access on container.");
                actionContext.Result = new ObjectResult("No access on containder") { StatusCode = (int)HttpStatusCode.Forbidden };
            }
        }
    }

    public class ValidEditReportProfileParameterActionFilter : BaseActionFilter
    {
        private RALogger logger = RALogger.GetInstance(typeof(ValidEditReportProfileParameterActionFilter));
        private IRMReportService mReportService;
        protected IRMReportService RMReportService
        {
            get
            {
                if (mReportService == null)
                {
                    mReportService = (IRMReportService)PlatformWindsorManager.GetService(typeof(IRMReportService));
                }
                return mReportService;
            }
        }
        public ValidEditReportProfileParameterActionFilter()
        {
        }
        protected override async Task OnActionAuthenticatedAsync(ActionExecutingContext actionContext)
        {
            var dto = actionContext.ActionArguments.Values.FirstOrDefault() as RMProfileDto;
            if (dto == null ||  dto?.Type == null)
            {
                logger.Warn($"dto or dto.type is null");
                actionContext.Result = new ObjectResult("Invalid dto or dto.type") { StatusCode = (int)HttpStatusCode.Forbidden };
                return;
            }
            ValidReportUtil util = new ValidReportUtil();
            var realProfile = await RMReportService.GetProfileByIdAsync(dto?.Id.ToString());
               
            bool isAdmin = await util.IsAdminAsync();
            bool isSOAdmin = await util.IsSOAdminAsync();
            if (!(isAdmin && await util.ValidatePermissionByJobTypeAsync((int)dto.Type, isAdmin))
                && !(isSOAdmin && await util.ValidateSOPermissionByJobTypeAsync((int)dto.Type, isSOAdmin)))
            {
                if (!(await util.ValidateTermAsync(dto)))
                {
                    logger.Warn($"Current user has no access on term.");
                    actionContext.Result = new ObjectResult("No access on term") { StatusCode = (int)HttpStatusCode.Forbidden };
                    return;
                }
                if (!realProfile.CreateProfileUserId.Equals(TenantLocalValue.LogonUserId, StringComparison.OrdinalIgnoreCase))
                {
                    logger.Warn($"Current user has no access for profile: {realProfile.ProfileName}");
                    actionContext.Result = new ObjectResult("Invalid user id") { StatusCode = (int)HttpStatusCode.Forbidden };
                    return;
                }
                if (!(await util.ValidatePermissionByJobTypeAsync((int)dto.Type, isAdmin))
                    && !(await util.ValidateSOPermissionByJobTypeAsync((int)dto.Type, isSOAdmin)))
                {
                    logger.Warn($"Current user has no permission for this type:{dto.Type}");
                    actionContext.Result = new ObjectResult("Invalid permission") { StatusCode = (int)HttpStatusCode.Forbidden };
                    return;
                }
            }
            if (!(await util.ValidateParameterAsync(dto, isAdmin, JobTypeConstants.EXOReportTypes.Contains((int)dto.Type)))
                && !(await util.ValidateSOParameterAsync(dto, isSOAdmin, false)))
            {
                logger.Warn($"Current user has no access on container.");
                actionContext.Result = new ObjectResult("No access on container") { StatusCode = (int)HttpStatusCode.Forbidden };
            }
        }
    }

    public class ValidDeleteReportProfileParameterActionFilter : BaseActionFilter
    {
        private RALogger logger = RALogger.GetInstance(typeof(ValidDeleteReportProfileParameterActionFilter));
        private IRMReportService mReportService;
        protected IRMReportService RMReportService
        {
            get
            {
                if (mReportService == null)
                {
                    mReportService = (IRMReportService)PlatformWindsorManager.GetService(typeof(IRMReportService));
                }
                return mReportService;
            }
        }
        public ValidDeleteReportProfileParameterActionFilter()
        {
        }
        protected override async Task OnActionAuthenticatedAsync(ActionExecutingContext actionContext)
        {
            var dto = actionContext.ActionArguments.Values.FirstOrDefault() as DelProfileInfo;
            if (dto == null || dto?.Ids == null || dto?.Type == null)
            {
                logger.Warn($"dto or dto.ids or dto.type is null");
                actionContext.Result = new ObjectResult("Invalid dto or dto.ids or dto.type") { StatusCode = (int)HttpStatusCode.Forbidden };
                return;
            }
            ValidReportUtil util = new ValidReportUtil();
            bool isAdmin = await util.IsAdminAsync();
            bool isSOAdmin = await util.IsSOAdminAsync();
            if (!(isAdmin && await util.ValidatePermissionByJobTypeAsync((int)dto.Type, isAdmin))
                && !(isSOAdmin && await util.ValidateSOPermissionByJobTypeAsync((int)dto.Type, isSOAdmin)))
            {                           
               foreach (var id in dto.Ids)
               {
                    var realProfile = await RMReportService.GetProfileByIdAsync(id.ToString());
                    if (!realProfile.CreateProfileUserId.Equals(TenantLocalValue.LogonUserId, StringComparison.OrdinalIgnoreCase))
                    {
                        logger.Warn($"Current user has no access for profile: {realProfile.ProfileName}");
                        actionContext.Result = new ObjectResult("Invalid user id") { StatusCode = (int)HttpStatusCode.Forbidden };
                        break;
                    }
                    if (!(await util.ValidatePermissionByJobTypeAsync((int)realProfile.Type, isAdmin))
                        && !(await util.ValidateSOPermissionByJobTypeAsync((int)realProfile.Type, isSOAdmin)))
                    {
                        logger.Warn($"Current user has no permission for this type:{realProfile.Type}");
                        actionContext.Result = new ObjectResult("Invalid permission") { StatusCode = (int)HttpStatusCode.Forbidden };
                        break;
                    }
                    if (!(await util.ValidateParameterAsync(realProfile, true))
                        && !(await util.ValidateSOParameterAsync(realProfile, true)))
                    {
                        logger.Warn($"Current user has no access on container.");
                        actionContext.Result = new ObjectResult("No access on containe") { StatusCode = (int)HttpStatusCode.Forbidden };
                        break;
                    }
                }
            }
        }
    }

    public class ValidReportIdParameterActionFilter : BaseActionFilter
    {
        private RALogger logger = RALogger.GetInstance(typeof(ValidReportIdParameterActionFilter));
        private IRMReportService mReportService;
        protected IRMReportService RMReportService
        {
            get
            {
                if (mReportService == null)
                {
                    mReportService = (IRMReportService)PlatformWindsorManager.GetService(typeof(IRMReportService));
                }
                return mReportService;
            }
        }
        public ValidReportIdParameterActionFilter()
        {
        }
        protected override async Task OnActionAuthenticatedAsync(ActionExecutingContext actionContext)
        {
            var id = actionContext.ActionArguments.Values.FirstOrDefault()?.ToString();
            ValidReportUtil util = new ValidReportUtil();
            var realProfile = await RMReportService.GetProfileByIdAsync(id);
            var isAdmin = await util.IsAdminAsync();
            bool isSOAdmin = await util.IsSOAdminAsync();
            if (!(isAdmin && await util.ValidatePermissionByJobTypeAsync((int)realProfile.Type, isAdmin))
                && !(isSOAdmin && await util.ValidateSOPermissionByJobTypeAsync((int)realProfile.Type, isSOAdmin)))
            {
                if (!realProfile.CreateProfileUserId.Equals(TenantLocalValue.LogonUserId, StringComparison.OrdinalIgnoreCase))
                {
                    logger.Warn($"Current user has no access for profile: {realProfile.ProfileName}");
                    actionContext.Result = new ObjectResult("Invalid user id") { StatusCode = (int)HttpStatusCode.Forbidden };
                    return;
                }
                if (!(await util.ValidatePermissionByJobTypeAsync((int)realProfile.Type, isAdmin))
                    && !(await util.ValidateSOPermissionByJobTypeAsync((int)realProfile.Type, isSOAdmin)))
                {
                    logger.Warn($"Current user has no permission for this type:{realProfile.Type}");
                    actionContext.Result = new ObjectResult("Invalid permission") { StatusCode = (int)HttpStatusCode.Forbidden };
                }
            }


            //由于可能出现先创建profile再移除container权限的情况，不在这里校验user是否对container有权限，否则可能会导致无法看到profile
            //else
            //{
            //    if (!SecurityTrimmingHelper.DoesUserHasThisPermission(util.GetPermissionMasks((int)realProfile.Type), true))
            //    {
            //        if (!util.ValidateParameter(realProfile))
            //        {
            //            logger.Warn($"Current user has no access on container.");
            //            actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Forbidden, "Invalid parameter");
            //        }
            //    }
            //}
        }
    }


    public class ValidShowReportQueryPagerActionFilter : BaseActionFilter
    {
        private RALogger logger = RALogger.GetInstance(typeof(ValidShowReportQueryPagerActionFilter));
        private IRMReportService mReportService;
        protected IRMReportService RMReportService
        {
            get
            {
                if (mReportService == null)
                {
                    mReportService = (IRMReportService)PlatformWindsorManager.GetService(typeof(IRMReportService));
                }
                return mReportService;
            }
        }
        public ValidShowReportQueryPagerActionFilter()
        {
        }
        protected override async Task OnActionAuthenticatedAsync(ActionExecutingContext actionContext)
        {
            var dto = actionContext.ActionArguments.Values.FirstOrDefault() as ShowReportQuery;
            ValidReportUtil util = new ValidReportUtil();
            var realProfile = await RMReportService.GetProfileByIdAsync(dto?.ProfileId.ToString());
            bool isAdmin = await util.IsAdminAsync();
            bool isSOAdmin = await util.IsSOAdminAsync();
            if (!(isAdmin && await util.ValidatePermissionByJobTypeAsync((int)realProfile.Type, isAdmin))
                && !(isSOAdmin && await util.ValidateSOPermissionByJobTypeAsync((int)realProfile.Type, isSOAdmin)))
            {
                if (!realProfile.CreateProfileUserId.Equals(TenantLocalValue.LogonUserId, StringComparison.OrdinalIgnoreCase))
                {
                    logger.Warn($"Current user has no access for profile: {realProfile.ProfileName}");
                    actionContext.Result = new ObjectResult("Invalid user id") { StatusCode = (int)HttpStatusCode.Forbidden };
                }
            }
        }
    }

    public class ValidReportUtil
    {
        private RALogger logger = RALogger.GetInstance(typeof(ValidReportUtil));
        public RMScopeRoleAssignmentDao RMScopeRoleAssignmentDao = new RMScopeRoleAssignmentDao();
        private IRMReportService mReportService;
        protected IRMReportService RMReportService
        {
            get
            {
                if (mReportService == null)
                {
                    mReportService = (IRMReportService)PlatformWindsorManager.GetService(typeof(IRMReportService));
                }
                return mReportService;
            }
        }
        private ISPSettingTreeService mSPSettingTreeService;
        protected ISPSettingTreeService SPSettingTreeService
        {
            get
            {
                if (mSPSettingTreeService == null)
                {
                    mSPSettingTreeService = (ISPSettingTreeService)PlatformWindsorManager.GetService(typeof(ISPSettingTreeService));
                }
                return mSPSettingTreeService;
            }
        }

        private ITeamsSettingTreeService mTeamsSettingTreeService;
        protected ITeamsSettingTreeService TeamsSettingTreeService
        {
            get
            {
                if (mTeamsSettingTreeService == null)
                {
                    mTeamsSettingTreeService = (ITeamsSettingTreeService)PlatformWindsorManager.GetService(typeof(ITeamsSettingTreeService));
                }
                return mTeamsSettingTreeService;
            }
        }
        private ILocationManagementService mLocationManagementService;
        protected ILocationManagementService LocationManagementService
        {
            get
            {
                if (mLocationManagementService == null)
                {
                    mLocationManagementService = (ILocationManagementService)PlatformWindsorManager.GetService(typeof(ILocationManagementService));
                }
                return mLocationManagementService;
            }
        }
        public ITermDao TermDao
        {
            get
            {
                return (ITermDao)PlatformWindsorManager.GetService(typeof(ITermDao));
            }
        }

        public ITermSetDao TermSetDao
        {
            get
            {
                return (ITermSetDao)PlatformWindsorManager.GetService(typeof(ITermSetDao));
            }
        }

        public ISecurityGroupManagementService SecurityGroupManagementService
        {
            get
            {
                return (ISecurityGroupManagementService)PlatformWindsorManager.GetService(typeof(ISecurityGroupManagementService));
            }
        }

        private IUserService userService = new UserService();

        public IRMSecurityTrimmingHelper SecurityTrimmingHelper
        {
            get
            {
                return (IRMSecurityTrimmingHelper)PlatformWindsorManager.GetService(typeof(IRMSecurityTrimmingHelper));
            }
        }

        private IRMRemoteNodeDao mRMRemoteNodeDao;
        protected IRMRemoteNodeDao RMRemoteNodeDao
        {
            get
            {
                if (mRMRemoteNodeDao == null)
                {
                    mRMRemoteNodeDao = (IRMRemoteNodeDao)PlatformWindsorManager.GetService(typeof(IRMRemoteNodeDao));
                }
                return mRMRemoteNodeDao;
            }
        }

        private IRMMailboxDao mMailBoxDao;
        protected IRMMailboxDao MailBoxDao
        {
            get
            {
                if (mMailBoxDao == null)
                {
                    mMailBoxDao = (IRMMailboxDao)PlatformWindsorManager.GetService(typeof(IRMMailboxDao));
                }
                return mMailBoxDao;
            }
        }
        public async Task<bool> ValidatePermissionByJobTypeAsync(int jobType, bool isAdmin)
        {
            bool isValidPermission = false;
            bool isValidJobType = false;

            if (JobTypeConstants.SPReportTypes.Contains(jobType))
            {
                isValidJobType = true;
                if (await CheckPermissionAsync(RMPermissionMasks.SPOEnduser))
                {
                    isValidPermission = true;
                }
                else
                {
                    logger.Info("Report job doesn't have spo permission");
                }
            }
            if (JobTypeConstants.EXOReportTypes.Contains(jobType))
            {
                isValidJobType = true;
                if (await CheckPermissionAsync(RMPermissionMasks.EXOEnduser))
                {
                    isValidPermission = true;
                }
                else
                {
                    logger.Info("Report job doesn't have exo permission");
                }
            }

            if (JobTypeConstants.PhysicalReportTypes.Contains(jobType))
            {
                isValidJobType = true;
                if (await CheckPermissionAsync(RMPermissionMasks.PhysicalEndUser))
                {
                    isValidPermission = true;
                }
                else
                {
                    logger.Info("Report job doesn't have physical permission");
                }
            }

            if (JobTypeConstants.FSReportTypes.Contains(jobType))
            {
                isValidJobType = true;
                if (await CheckPermissionAsync(RMPermissionMasks.FSEnduser))
                {

                    isValidPermission = true;
                }
                else
                {
                    logger.Info("Report job doesn't have fs permission");
                }
            }

            if(JobTypeConstants.BoxReportTypes.Contains(jobType))
            {
                isValidJobType = true;
                if (await CheckPermissionAsync(RMPermissionExtensionMasks.BoxEndUser))
                {
                isValidPermission = true;
            }
                else
                {
                    logger.Info("Report job doesn't have box permission");
                }
            }

            if (JobTypeConstants.GoogleReportTypes.Contains(jobType))
            {
                isValidJobType = true;
                if (await CheckPermissionAsync(RMPermissionExtensionMasks.GoogleEndUser))
                {
                    isValidPermission = true;
                }
                else
                {
                    logger.Info("Report job doesn't have google permission");
                }
            }

            if (JobTypeConstants.OneDriveReportTypes.Contains(jobType))
            {
                isValidJobType = true;
                if (await CheckPermissionAsync(RMPermissionMasks.OneDriveEnduser))
                {
                    isValidPermission = true;
                }
                else
                {
                    logger.Info("Report job doesn't have onedrive permission");
                }
            }

            if (JobTypeConstants.SPOnPremReportTypes.Contains(jobType))
            {
                isValidJobType = true;
                if (await CheckPermissionAsync(RMPermissionMasks.SPOnPremEnduser))
                {
                    isValidPermission = true;
                }
                else
                {
                    logger.Info("Report job doesn't have SPOnPrem permission");
                }
            }

            if (JobTypeConstants.TeamsReportTypes.Contains(jobType))
            {
                isValidJobType = true;
                if (await CheckPermissionAsync(RMPermissionExtensionMasks.TeamsEndUser) && TeamsPermissionHelper.HasUpgradeTeamsFeature())
                {
                    isValidPermission = true;
                }
                else
                {
                    logger.Info("Report job doesn't have spo permission");
                }
            }

            if (!isValidJobType)
            {
                return false;
            }
            else
            {
                return isValidPermission;
            }
        }

        public async Task<bool> ValidateSOPermissionByJobTypeAsync(int jobType, bool isAdmin)
        {
            bool isValidPermission = false;
            bool isValidJobType = false;

            if (JobTypeConstants.SOSPReportTypes.Contains(jobType))
            {
                isValidJobType = true;
                if (await CheckSOPermissionAsync(RMSOPermissionMasks.SPOEnduser))
                {
                    isValidPermission = true;
                }
                else
                {
                    logger.Info("Report job doesn't have spo permission");
                }
            }

            if (JobTypeConstants.SOOneDriveReportTypes.Contains(jobType))
            {
                isValidJobType = true;
                if (await CheckSOPermissionAsync(RMSOPermissionMasks.OneDriveEnduser))
                {
                    isValidPermission = true;
                }
                else
                {
                    logger.Info("Report job doesn't have onedrive permission");
                }
            }

            if (JobTypeConstants.SOTeamsReportTypes.Contains(jobType))
            {
                isValidJobType = true;
                if (await CheckSOPermissionAsync(RMSOPermissionMasks.TeamsEndUser) && TeamsPermissionHelper.HasUpgradeTeamsFeature())
                {
                    isValidPermission = true;
                }
                else
                {
                    logger.Info("Report job doesn't have teams permission");
                }
            }

            if (!isValidJobType)
            {
                return false;
            }
            else if (isValidJobType && isAdmin)
            {
                return isValidJobType;
            }
            else
            {
                return isValidPermission;
            }
        }

        ///// <summary>
        ///// 检查user是否为admin，对于SP/EXO判断是否为SPAdmin/EXOAdmin，对于physical/FS判断是否为super admin
        ///// </summary>
        ///// <param name="jobType"></param>
        ///// <returns></returns>
        //private RMPermissionMasks GetPermissionMasks(int jobType)
        //{
        //    if (JobTypeConstants.SPReportTypes.Contains(jobType))
        //    {
        //        return RMPermissionMasks.SPOAdmin;
        //    }
        //    else if (JobTypeConstants.EXOReportTypes.Contains(jobType))
        //    {
        //        return RMPermissionMasks.EXOAdmin;
        //    }
        //    else if (JobTypeConstants.PhysicalReportTypes.Contains(jobType))
        //    {
        //        return RMPermissionMasks.AccessAll;
        //    }
        //    else if (JobTypeConstants.FSReportTypes.Contains(jobType))
        //    {
        //        return RMPermissionMasks.AccessAll;
        //    }
        //    else
        //    {
        //        throw new Exception("Invalid job type: " + jobType);
        //    }
        //}

        public async Task<bool> IsAdminAsync()
        {
            if (!(await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.ReportCenterAdmin)))
            {
                return false;
            }
            return true;
        }

        public async Task<bool> IsSOAdminAsync()
        {
            //暂时先这么顶着，so还没有ReportCenterAdmin这类权限
            if (!(await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMSOPermissionMasks.CommonModuleAccess))
                || !(await SecurityGroupManagementService.GetUserScopePermissionsAsync(TenantLocalValue.LogonUserId)).IsAdmin)
            {
                return false;
            }
            return true;
        }

        private Task<bool> CheckPermissionAsync(RMPermissionMasks permission)
        {
            return SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(permission);
        }

        private Task<bool> CheckPermissionAsync(RMPermissionExtensionMasks permission)
        {
            return SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(permission);
        }

        private Task<bool> CheckSOPermissionAsync(RMSOPermissionMasks permission)
        {
            return SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(permission);
        }

        public async Task<bool> ValidateTermAsync(RMProfileDto profile)
        {
            bool isValid = false;
            if (profile.Type == JobType.BCSTermUsageReport || profile.Type == JobType.EXOTermUsageReport
                || profile.Type == JobType.PhysicalTermUsageReport || profile.Type == JobType.FSBCSTermUsageReport || profile.Type == JobType.OneDriveTermUsageReport
                || profile.Type == JobType.BoxBCSTermUsageReport || profile.Type == JobType.GoogleBCSTermUsageReport)
            {
                if (string.IsNullOrEmpty(profile.Extension1))
                {
                    logger.Warn($"{profile?.ProfileName}, profile term tree is null");
                    return isValid;
                }

                Dictionary<int, RMTermDto> termDic = JsonConvert.DeserializeObject<Dictionary<int, RMTermDto>>(profile.Extension1);
                var termSetIds = GetTermSetIdsByProfileTree(termDic);
                
                if (!(await SecurityGroupManagementService.DoesUserHasPermisionToTermAsync(TenantLocalValue.LogonUserId, SecurityTermLevel.TermSet, termSetIds)))
                {
                    logger.Warn($"Report job doesn't have term permission, uid:{TenantLocalValue.LogonUserId} TermSetIds: {string.Join(";", termSetIds)}");
                    return isValid;
                }
               
            }
            return true;
        }

        private List<Guid> GetTermSetIdsByProfileTree(Dictionary<int, RMTermDto> termTree) 
        {
            List<Guid> result = new List<Guid>();
            List<int> termIds = new List<int>();
            foreach (var term in termTree.Values)
            {
                if (term.Type == "TermSet" && term.IsChecked)
                {
                    if (Guid.TryParse(term.UniqueId, out Guid termSetId))
                    {
                        result.Add(termSetId);
                    }
                }
                else if (term.Type == "Term" && term.IsChecked)
                {
                    termIds.Add(term.Id);
                }
            }
            var tsIds = TermDao.GetTermSetIdListByTermIds(termIds);
            result.AddRange(tsIds);
            return result.Distinct().ToList();
        }

        public async Task<bool> ValidateParameterAsync(RMProfileDto profile, bool isAdmin, bool needBuildTree = false)
        {
            bool isValid = false;
            if (JobTypeConstants.SPReportTypes.Contains((int)profile.Type))
            {
                if (!string.IsNullOrWhiteSpace(profile.Extension2))
                {
                    var treeNodes = string.Empty;
                    if (needBuildTree)
                    {
                        var spFarm = SPSettingTreeService.LoadFarm()[0];
                        treeNodes = SPTreeUtil.BuildSPTreeXMLStr(profile.Extension2, spFarm.FarmId);
                    }
                    else
                    {
                        treeNodes = profile.Extension2;
                    }
                    isValid = await ValidSPTreeAsync(treeNodes, SourceFlag.SharePoint, isAdmin);
                }
            }
            else if (JobTypeConstants.EXOReportTypes.Contains((int)profile.Type))
            {
                if (!isAdmin)
                {
                    if (!string.IsNullOrWhiteSpace(profile.Extension2))
                    {
                        var treeNodes = string.Empty;
                        if (needBuildTree)
                        {
                            var EXORoot = SPSettingTreeService.LoadExchangeRoot()[0];
                            treeNodes = SPTreeUtil.BuildEXOTreeXMLStr(profile.Extension2, EXORoot.Id);
                        }
                        else
                        {
                            treeNodes = profile.Extension2;
                        }
                        isValid = await ValidEXOTreeAsync(treeNodes);
                    }
                }
                else
                {
                    isValid = true;
                }
            }
            else if (JobTypeConstants.OneDriveReportTypes.Contains((int)profile.Type))
            {
                if (!string.IsNullOrWhiteSpace(profile.Extension2))
                {
                    var treeNodes = string.Empty;
                    if (needBuildTree)
                    {
                        treeNodes = SPTreeUtil.BuildSPTreeXMLStr(profile.Extension2, "");
                    }
                    else
                    {
                        treeNodes = profile.Extension2;
                    }
                    isValid = await ValidSPTreeAsync(treeNodes, SourceFlag.OneDrive, isAdmin);
                }
            }
            else if (JobTypeConstants.TeamsReportTypes.Contains((int)profile.Type) && TeamsPermissionHelper.HasUpgradeTeamsFeature())
            {
                if (!string.IsNullOrWhiteSpace(profile.Extension2))
                {
                    var treeNodes = string.Empty;
                    if (needBuildTree)
                    {
                        treeNodes = SPTreeUtil.BuildSPTreeXMLStr(profile.Extension2, "");
                    }
                    else
                    {
                        treeNodes = profile.Extension2;
                    }
                    isValid = await ValidTeamsTreeAsync(treeNodes, SourceFlag.Teams, isAdmin);
                }
            }
            else if (JobTypeConstants.PhysicalReportTypes.Contains((int)profile.Type))
            {
                isValid = true;
            }
            else if (JobTypeConstants.FSReportTypes.Contains((int)profile.Type))
            {
                isValid = true;
            }
            else if (JobTypeConstants.SPOnPremReportTypes.Contains((int)profile.Type))
            {
                isValid = true;
            }
            else if (JobTypeConstants.BoxReportTypes.Contains((int)profile.Type))
            {
                isValid = true;
            }
            else if (JobTypeConstants.GoogleReportTypes.Contains((int)profile.Type))
            {
                isValid = true;
            }
            else
            {
                isValid = false;
            }
            return isValid;
        }

        public async Task<bool> ValidateSOParameterAsync(RMProfileDto profile, bool isAdmin, bool needBuildTree = false)
        {
            bool isValid = false;
            if (JobTypeConstants.SOSPReportTypes.Contains((int)profile.Type))
            {
                if (!string.IsNullOrWhiteSpace(profile.Extension2))
                {
                    var treeNodes = string.Empty;
                    if (needBuildTree)
                    {
                        var spFarm = SPSettingTreeService.LoadFarm()[0];
                        treeNodes = SPTreeUtil.BuildSPTreeXMLStr(profile.Extension2, spFarm.FarmId);
                    }
                    else
                    {
                        treeNodes = profile.Extension2;
                    }
                    isValid = await ValidSPTreeAsync(treeNodes, SourceFlag.SharePoint, isAdmin);
                }
            }
            else if (JobTypeConstants.SOOneDriveReportTypes.Contains((int)profile.Type))
            {
                if (!string.IsNullOrWhiteSpace(profile.Extension2))
                {
                    var treeNodes = string.Empty;
                    if (needBuildTree)
                    {

                        treeNodes = SPTreeUtil.BuildSPTreeXMLStr(profile.Extension2, "");
                    }
                    else
                    {
                        treeNodes = profile.Extension2;
                    }
                    isValid = await ValidSPTreeAsync(treeNodes, SourceFlag.OneDrive, isAdmin);
                }
            }
            else if (JobTypeConstants.SOTeamsReportTypes.Contains((int)profile.Type) && TeamsPermissionHelper.HasUpgradeTeamsFeature())
            {
                if (!string.IsNullOrWhiteSpace(profile.Extension2))
                {
                    var treeNodes = string.Empty;
                    if (needBuildTree)
                    {

                        treeNodes = SPTreeUtil.BuildSPTreeXMLStr(profile.Extension2, "");
                    }
                    else
                    {
                        treeNodes = profile.Extension2;
                    }
                    isValid = await ValidTeamsTreeAsync(treeNodes, SourceFlag.Teams, isAdmin);
                }
            }
            else
            {
                isValid = false;
            }
            return isValid;
        }

        public async Task<bool> ValidSPTreeAsync(string treeStr, SourceFlag flag, bool isAdmin)
        {
            var treeNode = SerializerHelper.DeserializeByJsonSerializer<RMSPTreeNode>(treeStr, true);
            var groupNodes = GetWebApplications(treeNode);
            if (groupNodes == null)
            {
                logger.Info("Report job tree doesn't contain web application.");
                return false;
            }
            foreach (var webApp in groupNodes)
            {
                var webApplication = RABrowserClient.GetWebApplicationById(webApp.SPObjectId);
                if (webApplication != null)
                {
                    if ((webApplication.NodeType == GCommon.Contract.Server.ControlPanel.Office365.RemoveNodeType.SkyDrivePro
                        && flag != SourceFlag.OneDrive)
                        || (webApplication.NodeType != GCommon.Contract.Server.ControlPanel.Office365.RemoveNodeType.SkyDrivePro
                        && flag == SourceFlag.OneDrive))
                    {
                        logger.Info("Report job tree doesn't match data source.");
                        return false;
                    }
                }
            }
            if (!isAdmin)
            {
                treeNode = SPTreeUtil.BuildSPTree(SPTreeUtil.ConvertTreeStrToSPTreeJsonStr(treeStr));
                foreach (var node in treeNode.Children)
                {
                    if (node.Level >= (int)NodeLevel.WebApplication)
                    {
                        //Guid containerId = node.SiteGroupId;
                        //List<string> userAndGroupUserIds = userService.GetUserAndGroupUserIds(TenantLocalValue.LogonUserId);
                        //if (!RMScopeRoleAssignmentDao.HavePermissionOnContainerId(containerId, userAndGroupUserIds))
                        //{
                        //    isValid = false;
                        //    break;
                        //}
                        if (!(await IsSPChildValidAsync(node)))
                        {
                            logger.Info("Report job doesn't have container permission");
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        public async Task<bool> ValidTeamsTreeAsync(string treeStr, SourceFlag flag, bool isAdmin)
        {
            var treeNode = SerializerHelper.DeserializeByJsonSerializer<RMSPTreeNode>(treeStr, true);
            var groupNodes = GetWebApplications(treeNode);
            if (groupNodes == null)
            {
                logger.Info("Report job tree doesn't contain web application.");
                return false;
            }
            if (!isAdmin)
            {
                treeNode = SPTreeUtil.BuildSPTree(SPTreeUtil.ConvertTreeStrToSPTreeJsonStr(treeStr));
                foreach (var node in treeNode.Children)
                {
                    if (node.Level >= (int)NodeLevel.WebApplication)
                    {
                        if (!(await IsSPChildValidAsync(node)))
                        {
                            logger.Info("Report job doesn't have container permission");
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private List<RMSPTreeNode> GetWebApplications(RMSPTreeNode treeNode)
        {
            var webApps = treeNode.Children.ToList();
            if (webApps != null && webApps.Count > 0)
            {
                return webApps;
            }
            else
            {
                return null;
            }
        }

        private async Task<bool> IsSPChildValidAsync(RMSPTreeNode node)
        {
            if (node.CheckNumber == 1)
            {
                string containerId = TreeNodeUtil.GetSPContainderId(node);
                List<string> userAndGroupUserIds = await userService.GetUserAndGroupUserIdsAsync(TenantLocalValue.LogonUserId);
                if (!RMScopeRoleAssignmentDao.HavePermissionOnContainerId(new Guid(containerId), userAndGroupUserIds))
                {
                    return false;
                }
            }
            if (node.Children != null && node.Children.Count > 0)
            {
                foreach (var subNode in node.Children)
                {
                    if (!(await IsSPChildValidAsync(subNode)))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public async Task<bool> ValidEXOTreeAsync(string treeStr)
        {
            var farmTree = SerializerHelper.DeserializeByJsonSerializer<RMEXOTreeNode>(treeStr, true);
            foreach (var node in farmTree.Children)
            {
                //if (node.Level != (int)NodeLevel.ExchangeOnlineFarm)
                //{
                //    string containerId = TreeNodeUtil.GetEXOContainderId(node);
                //    List<string> userAndGroupUserIds = userService.GetUserAndGroupUserIds(TenantLocalValue.LogonUserId);
                //    if (!RMScopeRoleAssignmentDao.HavePermissionOnContainerId(new Guid(containerId), userAndGroupUserIds))
                //    {
                //        isValid = false;
                //        break;
                //    }
                //}
                if (!(await IsEXOChildValidAsync(node)))
                {
                    return false;
                }
            }
            return true;
        }

        private async Task<bool> IsEXOChildValidAsync(RMEXOTreeNode node)
        {
            if (node.CheckNumber == 1)
            {
                string containerId = TreeNodeUtil.GetEXOContainderId(node);
                List<string> userAndGroupUserIds = await userService.GetUserAndGroupUserIdsAsync(TenantLocalValue.LogonUserId);
                if (!RMScopeRoleAssignmentDao.HavePermissionOnContainerId(new Guid(containerId), userAndGroupUserIds))
                {
                    return false;
                }
            }

            if (node.Children != null && node.Children.Count > 0)
            {
                foreach (var subNode in node.Children)
                {
                    if (!(await IsEXOChildValidAsync(subNode)))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        public async Task<string> GetFilteredTermNodesAsync(string termStr)
        {
            var term = SerializerHelper.DeserializeByJsonSerializer<List<RMTermDto>>(termStr);
            if (!term.IsNullOrEmpty() && term[0].Type == "TermGroup")
            {
                List<RMTermDto> termsets = term[0].subTerms;
                List<RMTermDto> newSubTerms = new List<RMTermDto>();
                if (!termsets.IsNullOrEmpty())
                {
                    foreach (RMTermDto termset in termsets)
                    {
                        List<Guid> termSetId = new List<Guid>();
                        termSetId.Add(new Guid(termset.UniqueId));
                        bool hasPermission = await SecurityGroupManagementService.DoesUserHasPermisionToTermAsync(TenantLocalValue.LogonUserId, SecurityTermLevel.TermSet, termSetId);
                        if (hasPermission)
                        {
                            newSubTerms.Add(termset);
                        }
                    }
                    term[0].subTerms = newSubTerms;
                }
            }
            return SerializerHelper.SerializeByJsonSerializer(term);
        }
        public async Task<string> GetFilteredSPTreeNodesAsync(string nodesStr, JobType jobType)
        {
            //var nodes = serializer.Deserialize<List<RMSPTreeNode>>(nodesStr);
            var nodes = SerializerHelper.DeserializeByJsonConvert<List<RMSPTreeNode>>(nodesStr);
            var spFarm = SPSettingTreeService.LoadFarm()[0];
            var farmTreeStr = SPTreeUtil.BuildSPTreeXMLStr(nodesStr, spFarm.FarmId);
            var farmTreeNode = SerializerHelper.DeserializeByJsonSerializer<RMSPTreeNode>(farmTreeStr, true);
            var filterOutOneDriveNodeId = new List<string>();
            foreach (RMSPTreeNode node in nodes)
            {
                if (FilteredOutOneDriveSPNode(node))
                {
                    filterOutOneDriveNodeId.Add(node.Id);
                }
            }

            if (((await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.SPOAdmin))&& JobTypeConstants.SPReportTypes.Contains((int)jobType))
            || ((await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMSOPermissionMasks.SPOAdmin))&& JobTypeConstants.SOSPReportTypes.Contains((int)jobType)))
            {
                //return nodesStr;
                var spNodes = nodes.Where(n => !filterOutOneDriveNodeId.Contains(n.Id)).ToList();
                var SPTree = SPTreeUtil.BuildSPTreeJsonStr(spNodes);
                return SPTree;
                //return serializer.Serialize(nodes.Where(n => !filterOutOneDriveNodeId.Contains(n.Id)).ToList());
            }

            List<string> mFilteredSPNodeIds = new List<string>();
            mFilteredSPNodeIds.Add(farmTreeNode.Id);
            foreach (var node in farmTreeNode.Children)
            {
                await GetFilteredSPNodeAsync(node, mFilteredSPNodeIds);
            }
            //List<RMSPTreeNode> filteredNodes = new List<RMSPTreeNode>();
            //foreach (var node in nodes)
            //{
            //    if (node.Level >= (int)NodeLevel.WebApplication)
            //    {
            //        if (HasPermissionOnSPNode(node))
            //        {
            //            filteredNodes.Add(node);
            //        }
            //    }
            //}
            var spNodes2 = nodes.Where(n => !filterOutOneDriveNodeId.Contains(n.Id) && mFilteredSPNodeIds.Contains(n.Id)).ToList();
            var SPTree2 = SPTreeUtil.BuildSPTreeJsonStr(spNodes2);
            return SPTree2;
        }

        public async Task<string> GetFilteredTeamsTreeNodesAsync(string nodesStr, JobType jobType)
        {
            //var nodes = serializer.Deserialize<List<RMSPTreeNode>>(nodesStr);
            var nodes = SerializerHelper.DeserializeByJsonConvert<List<RMSPTreeNode>>(nodesStr);
            var spFarm = TeamsSettingTreeService.LoadFarm()[0];
            var farmTreeStr = SPTreeUtil.BuildSPTreeXMLStr(nodesStr, spFarm.FarmId);
            var farmTreeNode = SerializerHelper.DeserializeByJsonSerializer<RMSPTreeNode>(farmTreeStr, true);
            var filterOutOneDriveNodeId = new List<string>();
            foreach (RMSPTreeNode node in nodes)
            {
                if (FilteredOutOneDriveSPNode(node))
                {
                    filterOutOneDriveNodeId.Add(node.Id);
                }
            }

            if (((await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.TeamsAdmin)) && JobTypeConstants.TeamsReportTypes.Contains((int)jobType))
            || ((await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMSOPermissionMasks.TeamsAdmin)) && JobTypeConstants.SOTeamsReportTypes.Contains((int)jobType)))
            {
                //return nodesStr;
                var spNodes = nodes.Where(n => !filterOutOneDriveNodeId.Contains(n.Id)).ToList();
                var SPTree = SPTreeUtil.BuildSPTreeJsonStr(spNodes);
                return SPTree;
            }

            List<string> mFilteredSPNodeIds = new List<string>();
            mFilteredSPNodeIds.Add(farmTreeNode.Id);
            foreach (var node in farmTreeNode.Children)
            {
                await GetFilteredSPNodeAsync(node, mFilteredSPNodeIds);
            }
            var spNodes2 = nodes.Where(n => !filterOutOneDriveNodeId.Contains(n.Id) && mFilteredSPNodeIds.Contains(n.Id)).ToList();
            var SPTree2 = SPTreeUtil.BuildSPTreeJsonStr(spNodes2);
            return SPTree2;
        }


        public async Task<string> GetFilteredOneDriveTreeNodesAsync(string nodesStr, JobType jobType)
        {
            //var nodes = serializer.Deserialize<List<RMSPTreeNode>>(nodesStr);
            var nodes = SerializerHelper.DeserializeByJsonConvert<List<RMSPTreeNode>>(nodesStr);
            var spFarm = SPSettingTreeService.LoadFarm()[0];
            var farmTreeStr = SPTreeUtil.BuildSPTreeXMLStr(nodesStr, spFarm.FarmId);
            var farmTreeNode = SerializerHelper.DeserializeByJsonSerializer<RMSPTreeNode>(farmTreeStr, true);

            if (((await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.OneDriveAdmin))&& JobTypeConstants.OneDriveReportTypes.Contains((int)jobType))
                || ((await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMSOPermissionMasks.OneDriveAdmin))&& JobTypeConstants.SOOneDriveReportTypes.Contains((int)jobType)))
            {
                //return nodesStr;
                //return serializer.Serialize(nodes);
                //var spNodes = nodes.Where(n => !filterOutOneDriveNodeId.Contains(n.Id)).ToList();
                var SPTree = SPTreeUtil.BuildSPTreeJsonStr(nodes);
                return SPTree;
            }

            List<string> mFilteredSPNodeIds = new List<string>();
            mFilteredSPNodeIds.Add(farmTreeNode.Id);
            foreach (var node in farmTreeNode.Children)
            {
                await GetFilteredSPNodeAsync(node, mFilteredSPNodeIds);
            }
            //List<RMSPTreeNode> filteredNodes = new List<RMSPTreeNode>();
            //foreach (var node in nodes)
            //{
            //    if (node.Level >= (int)NodeLevel.WebApplication)
            //    {
            //        if (HasPermissionOnSPNode(node))
            //        {
            //            filteredNodes.Add(node);
            //        }
            //    }
            //}
            var spNodes2 = nodes.Where(n => mFilteredSPNodeIds.Contains(n.Id)).ToList();
            var SPTree2 = SPTreeUtil.BuildSPTreeJsonStr(spNodes2);
            return SPTree2;
            //return serializer.Serialize(nodes.Where(n => mFilteredSPNodeIds.Contains(n.Id)).ToList());
        }
        private async Task GetFilteredSPNodeAsync(RMSPTreeNode node, List<string> mFilteredSPNodeIds)
        {
            if (node.Level < (int)NodeLevel.WebApplication)
            {
                mFilteredSPNodeIds.Add(node.Id);
            }
            else
            {
                if (await HasPermissionOnSPNodeAsync(node))
                {
                    mFilteredSPNodeIds.Add(node.Id);
                }
            }
            if (node.Children != null && node.Children.Count > 0)
            {
                foreach (var subNode in node.Children)
                {
                    await GetFilteredSPNodeAsync(subNode, mFilteredSPNodeIds);
                }
            }
        }

        private bool FilteredOutOneDriveSPNode(RMSPTreeNode node)
        {
            return node.NodeType == (int)NodeType.SkyDriveProSitesGroup;
        }

        private async Task<bool> HasPermissionOnSPNodeAsync(RMSPTreeNode treeNode)
        {
            string containerId = TreeNodeUtil.GetSPContainderId(treeNode);
            List<string> userAndGroupUserIds = await userService.GetUserAndGroupUserIdsAsync(TenantLocalValue.LogonUserId);
            if (!RMScopeRoleAssignmentDao.HavePermissionOnContainerId(new Guid(containerId), userAndGroupUserIds))
            {
                return false;
            }

            return true;
        }

        public async Task<string> GetFilteredEXOTreeNodesAsync(string nodesStr)
        {
            if (await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.EXOAdmin))
            {
                return nodesStr;
            }
            var nodes = SerializerHelper.DeserializeByJsonConvert<List<RMEXOTreeNode>>(nodesStr);
            var EXORoot = SPSettingTreeService.LoadExchangeRoot()[0];
            var exoTreeStr = SPTreeUtil.BuildEXOTreeXMLStr(nodesStr, EXORoot.Id);
            var exoFarmNode = SerializerHelper.DeserializeByJsonSerializer<RMEXOTreeNode>(exoTreeStr, true);
            List<string> mFilteredEXONodeIds = new List<string>();
            mFilteredEXONodeIds.Add(exoFarmNode.Id);
            foreach (var node in exoFarmNode.Children)
            {
                await GetFilteredEXONodeAsync(node, mFilteredEXONodeIds);
            }
            return SerializerHelper.SerializeByJsonSerializer(nodes.Where(n => mFilteredEXONodeIds.Contains(n.Id)).ToList());
        }

        private async Task<bool> HasPermissionOnEXONodeAsync(RMEXOTreeNode treeNode)
        {
            string containerId = TreeNodeUtil.GetEXOContainderId(treeNode);
            List<string> userAndGroupUserIds = await userService.GetUserAndGroupUserIdsAsync(TenantLocalValue.LogonUserId);
            if (!RMScopeRoleAssignmentDao.HavePermissionOnContainerId(new Guid(containerId), userAndGroupUserIds))
            {
                return false;
            }
            return true;
        }

        private async Task GetFilteredEXONodeAsync(RMEXOTreeNode node, List<string> mFilteredEXONodeIds)
        {
            if (node.Level < (int)NodeLevel.ExchangeOnlineMailboxGroup)
            {
                mFilteredEXONodeIds.Add(node.Id);
            }
            else
            {
                if (await HasPermissionOnEXONodeAsync(node))
                {
                    mFilteredEXONodeIds.Add(node.Id);
                }
            }
            if (node.Children != null && node.Children.Count > 0)
            {
                foreach (var subNode in node.Children)
                {
                    await GetFilteredEXONodeAsync(subNode, mFilteredEXONodeIds);
                }
            }
        }
    }
}