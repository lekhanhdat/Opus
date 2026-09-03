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
using AvePoint.RA.Common.ClientRequest;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Helper;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Multi_Geo;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Account.Security;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.DB.SecurityTrimming.Model;
using AvePoint.RA.Service.Security;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Extentions.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft365.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Common.WIF
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    internal class RMApiAuthorizeAttribute : BaseAuthorizeAttribute
    {
        private static readonly AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(RMApiAuthorizeAttribute));
        public RMPermissionMasks RequiredPermission { get; private set; }
        public RMSOPermissionMasks RequiredSOPermission { get; private set; }
        public RMDiscoveryPermissionMasks RequiredDiscoveryPermission { get; private set; }
        public RMDiscoverySalesforcePermissionMask RequiredSalesforceDiscoveryPermission { get; private set; }
        public RMDiscoveryGoogleROTPermissionMask RequiredGoogleROTDiscoveryPermission { get; private set; }
        public RMDiscoveryFileSystemPermissionMask RequiredFSDiscoveryPermission { get; private set; }
        public PermissionJoinType permissionJoinType { get; set; } = PermissionJoinType.And;

        public PermissionJoinType DiffPermissionJoinType { get; set; } = PermissionJoinType.Any;
        public RMSubPermissionMasks RequiredSubPermission { get; private set; }
        public RMPermissionExtensionMasks RequiredPermissionExtention { get; private set; }
        public RMReportPermissionMasks RequiredReportPermission { get; private set; }
        public bool Preferred { get; set; }

        public bool NeedCheckPermission { get; set; } = true;

        public bool NeedNewOpusTenant { get; set; } = false;

        private IRMSecurityTrimmingHelper trimmingHelper => PlatformWindsorManager.GetService<IRMSecurityTrimmingHelper>();
        //private ITenantInfoDao TenantInfoDao => PlatformWindsorManager.GetService<ITenantInfoDao>();

        private ILicenseHelperService LicenseHelperService => PlatformWindsorManager.GetService<ILicenseHelperService>();
        private IMultiGeoSettingService MultiGeoSettingService => PlatformWindsorManager.GetService<IMultiGeoSettingService>();
        private static readonly ITenantService TenantService = PlatformWindsorManager.GetService<ITenantService>();
        /// <summary>
        /// permission check 
        /// </summary>
        /// <param name="requiredPermission"></param>
        /// <param name="andOr">
        /// default true, 
        /// true: requiredPermission中的权限是and关系, 需要都满足条件
        /// false:  requiredPermission中的权限是or关系, 满足一个条件即可
        /// </param>
        /// <param name="preferred">
        /// RMApiAuthorizeAttribute执行顺序是先执行Controller的权限认证，再执行Method的权限认证，当Method和Controller都加了RMApiAuthorizeAttribute，
        /// 只希望Method的认证生效，可以在Controller的RMApiAuthorizeAttribute上设置preferred: false
        /// /param>
        /// 
        public RMApiAuthorizeAttribute(RMDiscoveryPermissionMasks permissionMasks, PermissionJoinType joinType = PermissionJoinType.And, bool preferred = true)
        {
            RequiredDiscoveryPermission = permissionMasks;
            permissionJoinType = joinType;
            Preferred = preferred;
        }
        
        public RMApiAuthorizeAttribute(RMDiscoverySalesforcePermissionMask permissionMasks, PermissionJoinType joinType = PermissionJoinType.And, bool preferred = true)
        {
            RequiredSalesforceDiscoveryPermission = permissionMasks;
            permissionJoinType = joinType;
            Preferred = preferred;
        }
        public RMApiAuthorizeAttribute(RMDiscoveryGoogleROTPermissionMask permissionMasks, PermissionJoinType joinType = PermissionJoinType.And, bool preferred = true)
        {
            RequiredGoogleROTDiscoveryPermission = permissionMasks;
            permissionJoinType = joinType;
            Preferred = preferred;
        }
        public RMApiAuthorizeAttribute(RMDiscoveryFileSystemPermissionMask permissionMasks, PermissionJoinType joinType = PermissionJoinType.And, bool preferred = true)
        {
            RequiredFSDiscoveryPermission = permissionMasks;
            permissionJoinType = joinType;
            Preferred = preferred;
        }
        public RMApiAuthorizeAttribute(RMPermissionMasks requiredPermission, RMDiscoveryFileSystemPermissionMask fsDiscoveryPermission, PermissionJoinType joinType = PermissionJoinType.Any, PermissionJoinType diffPermissionJoinType = PermissionJoinType.Any, bool preferred = true)
        {
            RequiredPermission = requiredPermission;
            RequiredFSDiscoveryPermission = fsDiscoveryPermission;
            permissionJoinType = joinType;
            DiffPermissionJoinType = diffPermissionJoinType;
            Preferred = preferred;
        }
        public RMApiAuthorizeAttribute(RMDiscoverySalesforcePermissionMask permissionMasks, RMDiscoveryGoogleROTPermissionMask googleROTPermissionMasks, PermissionJoinType joinType = PermissionJoinType.And, bool preferred = true)
        {
            RequiredSalesforceDiscoveryPermission = permissionMasks;
            RequiredGoogleROTDiscoveryPermission = googleROTPermissionMasks;
            permissionJoinType = joinType;
            Preferred = preferred;
        }
        public RMApiAuthorizeAttribute(RMPermissionMasks requiredPermission, PermissionJoinType joinType = PermissionJoinType.And, bool preferred = true)
        {
            RequiredPermission = requiredPermission;
            permissionJoinType = joinType;
            Preferred = preferred;
        }

        public RMApiAuthorizeAttribute(RMPermissionMasks requiredPermission, RMPermissionExtensionMasks requieredPermissionExtension, PermissionJoinType joinType = PermissionJoinType.And, bool preferred = true)
        {
            RequiredPermission = requiredPermission;
            RequiredPermissionExtention = requieredPermissionExtension;
            DiffPermissionJoinType = joinType;
            Preferred = preferred;
        }
        public RMApiAuthorizeAttribute(RMPermissionMasks requiredPermission, RMReportPermissionMasks requieredreportPermission, PermissionJoinType joinType = PermissionJoinType.Any, bool preferred = true)
        {
            RequiredPermission = requiredPermission;
            RequiredReportPermission = requieredreportPermission;
            DiffPermissionJoinType = joinType;
            Preferred = preferred;
        }
        public RMApiAuthorizeAttribute(RMPermissionMasks requiredPermission, RMPermissionExtensionMasks requieredPermissionExtension, RMSOPermissionMasks requieredSOPermission, PermissionJoinType joinType = PermissionJoinType.Any, PermissionJoinType diffPermissionJoinType = PermissionJoinType.Any, bool preferred = true)
        {
            RequiredPermission = requiredPermission;
            RequiredPermissionExtention = requieredPermissionExtension;
            RequiredSOPermission = requieredSOPermission;
            permissionJoinType = joinType;
            DiffPermissionJoinType = diffPermissionJoinType;
            Preferred = preferred;
        }

        public RMApiAuthorizeAttribute(RMPermissionMasks permission, RMSubPermissionMasks subPermission, bool preferred = true)
        {
            RequiredPermission = permission;
            RequiredSubPermission = subPermission;
            Preferred = preferred;
        }

        public RMApiAuthorizeAttribute(RMPermissionExtensionMasks permissionExtension, bool preferred = true)
        {
            RequiredPermissionExtention = permissionExtension;
            Preferred = preferred;
        }

        public RMApiAuthorizeAttribute(RMSOPermissionMasks SOPermission, RMPermissionExtensionMasks permissionExtension, RMSubPermissionMasks subPermission, bool preferred = true)
        {
            RequiredSOPermission = SOPermission;
            RequiredPermissionExtention = permissionExtension;
            RequiredSubPermission = subPermission;
            Preferred = preferred;
        }
        
        public RMApiAuthorizeAttribute(RMSOPermissionMasks SOPermission, RMPermissionExtensionMasks permissionExtension,PermissionJoinType joinType = PermissionJoinType.And, bool preferred = true)
        {
            RequiredSOPermission = SOPermission;
            RequiredPermissionExtention = permissionExtension;
            permissionJoinType = joinType;
            Preferred = preferred;
        }

        public RMApiAuthorizeAttribute(RMPermissionMasks requiredPermission, RMSOPermissionMasks SOPermission, PermissionJoinType joinType = PermissionJoinType.Any, PermissionJoinType diffPermissionJoinType = PermissionJoinType.Any, bool preferred = true)
        {
            RequiredPermission = requiredPermission;
            RequiredSOPermission = SOPermission;
            permissionJoinType = joinType;
            DiffPermissionJoinType = diffPermissionJoinType;
            Preferred = preferred;
        }
        public RMApiAuthorizeAttribute(RMPermissionMasks requiredPermission, RMSOPermissionMasks SOPermission, RMReportPermissionMasks requieredreportPermission, PermissionJoinType joinType = PermissionJoinType.Any, PermissionJoinType diffPermissionJoinType = PermissionJoinType.Any, bool preferred = true)
        {
            RequiredPermission = requiredPermission;
            RequiredSOPermission = SOPermission;
            RequiredReportPermission = requieredreportPermission;
            permissionJoinType = joinType;
            DiffPermissionJoinType = diffPermissionJoinType;
            Preferred = preferred;
        }
        public RMApiAuthorizeAttribute(RMPermissionMasks requiredPermission, RMPermissionExtensionMasks permissionExtension, PermissionJoinType joinType, PermissionJoinType diffPermissionJoinType, bool preferred = true)
        {
            RequiredPermission = requiredPermission;
            RequiredPermissionExtention = permissionExtension;
            permissionJoinType = joinType;
            DiffPermissionJoinType = diffPermissionJoinType;
            Preferred = preferred;
        }
        public RMApiAuthorizeAttribute(RMSOPermissionMasks SOPermission, RMPermissionExtensionMasks permissionExtension, PermissionJoinType joinType, PermissionJoinType diffPermissionJoinType, bool preferred = true)
        {
            RequiredSOPermission = SOPermission;
            RequiredPermissionExtention = permissionExtension;
            permissionJoinType = joinType;
            DiffPermissionJoinType = diffPermissionJoinType;
            Preferred = preferred;
        }

        public RMApiAuthorizeAttribute(RMPermissionMasks requiredPermission, RMSOPermissionMasks SOPermission, RMDiscoveryPermissionMasks discoveryPermission, RMDiscoverySalesforcePermissionMask discoverySalesforcePermissionMask, PermissionJoinType joinType = PermissionJoinType.Any, PermissionJoinType diffPermissionJoinType = PermissionJoinType.Any, bool preferred = true)
        {
            RequiredPermission = requiredPermission;
            RequiredSOPermission = SOPermission;
            RequiredDiscoveryPermission = discoveryPermission;
            RequiredSalesforceDiscoveryPermission = discoverySalesforcePermissionMask;
            permissionJoinType = joinType;
            DiffPermissionJoinType = diffPermissionJoinType;
            Preferred = preferred;
        }
        public RMApiAuthorizeAttribute(RMPermissionMasks requiredPermission, RMSOPermissionMasks SOPermission, RMDiscoveryPermissionMasks discoveryPermission, RMDiscoverySalesforcePermissionMask discoverySalesforcePermissionMask, RMDiscoveryGoogleROTPermissionMask discoveryGoogleROTPermissionMask, RMDiscoveryFileSystemPermissionMask discoveryFileSystemPermissionMask,PermissionJoinType joinType = PermissionJoinType.Any, PermissionJoinType diffPermissionJoinType = PermissionJoinType.Any, bool preferred = true)
        {
            RequiredPermission = requiredPermission;
            RequiredSOPermission = SOPermission;
            RequiredDiscoveryPermission = discoveryPermission;
            RequiredSalesforceDiscoveryPermission = discoverySalesforcePermissionMask;
            RequiredGoogleROTDiscoveryPermission = discoveryGoogleROTPermissionMask;
            RequiredFSDiscoveryPermission = discoveryFileSystemPermissionMask;
            permissionJoinType = joinType;
            DiffPermissionJoinType = diffPermissionJoinType;
            Preferred = preferred;
        }

        public RMApiAuthorizeAttribute(RMPermissionMasks requiredPermission, RMSOPermissionMasks SOPermission, RMDiscoveryPermissionMasks discoveryPermission, RMDiscoveryGoogleROTPermissionMask discoveryGoogleROTPermissionMask, PermissionJoinType joinType = PermissionJoinType.Any, PermissionJoinType diffPermissionJoinType = PermissionJoinType.Any, bool preferred = true)
        {
            RequiredPermission = requiredPermission;
            RequiredSOPermission = SOPermission;
            RequiredDiscoveryPermission = discoveryPermission;
            RequiredGoogleROTDiscoveryPermission = discoveryGoogleROTPermissionMask;
            permissionJoinType = joinType;
            DiffPermissionJoinType = diffPermissionJoinType;
            Preferred = preferred;
        }

        public RMApiAuthorizeAttribute(RMPermissionMasks requiredPermission, RMSOPermissionMasks SOPermission, RMDiscoveryPermissionMasks discoveryPermission, RMPermissionExtensionMasks permissionExtension, PermissionJoinType joinType = PermissionJoinType.Any, bool preferred = true)
        {
            RequiredPermission = requiredPermission;
            RequiredSOPermission = SOPermission;
            RequiredDiscoveryPermission = discoveryPermission;
            RequiredPermissionExtention = permissionExtension;
            permissionJoinType = joinType;
            Preferred = preferred;
        }

        public RMApiAuthorizeAttribute(RMSOPermissionMasks SOPermission, PermissionJoinType joinType = PermissionJoinType.And, bool preferred = true)
        {
            RequiredSOPermission = SOPermission;
            permissionJoinType = joinType;
            Preferred = preferred;
        }

        public RMApiAuthorizeAttribute()
        {
            NeedCheckPermission = false;
        }

        protected override async Task<bool> IsAuthorizedAsync(AuthorizationFilterContext filterContext, RMIdentity Identity)
        {
            var httpContext = filterContext.HttpContext;
            Uri reqUrl = null;
            try
            {
                reqUrl = httpContext.Request.GetUrl();

                if (NeedNewOpusTenant && !TenantService.IsNewOpusTenant())
                {
                    logger.Warn($"user do not have new opus permission to access control:{reqUrl}");
                    return false;
                }

                if (!await IsMultiGeoIpAllowedAsync(filterContext, Identity))
                {
                    return false;
                }

                var actionDescriptor = filterContext.ActionDescriptor as ControllerActionDescriptor;
                //获取方法上添加的RMApiAuthorizeAttribute
                var methodAttributes = actionDescriptor.MethodInfo?.GetCustomAttributes(typeof(RMApiAuthorizeAttribute), true);
                AvePoint.GCommon.Utility.ArgumentCheck.NotNull(methodAttributes, nameof(methodAttributes));
                if (methodAttributes.Length > 0 && !this.Preferred)
                {
                    //方法添加的RMApiAuthorizeAttribute默认值都是true
                    //当this.Preferred为false时，说明当前this是Controller的RMApiAuthorizeAttribute
                    //当方法和Controller都加了RMApiAuthorizeAttribute，只验证方法的权限即可
                    return true;
                }

                //使用redis 从DB取Permission
                var userId = Identity.AccountId;
                if (string.IsNullOrEmpty(userId))
                {
                    logger.Warn($"user not found:{reqUrl}");
                    return false;
                }

                if (!NeedCheckPermission)
                {
                    return true;
                }

                var hasOpusILLicense = LicenseHelperService.HasOpusILLicense;
                var hasOpusSOLicense = LicenseHelperService.HasOpusSOLicense;
                var hasDiscoveryLicense = LicenseHelperService.HasOpusDiscoveryLicense;
                var hasGoogleLicense = LicenseHelperService.HasOpusGoogleLicense;
                var hasSalesforceDiscoveryLicense = LicenseHelperService.HasOpusSalesforceDiscoveryLicense;
                var hasGoogleROTLicense = LicenseHelperService.HasOpusGoogleROTDiscoveryLicense;
                var hasFSDiscoveryLicense = LicenseHelperService.HasOpusFileSystemDiscoveryLicense;
                PermissionChecker<RMPermissionMasks> opusILPermissionChecker = new(RequiredPermission, hasOpusILLicense || hasGoogleLicense, permissionJoinType);
                PermissionChecker<RMSubPermissionMasks> opusILSubPermissionChecker = new(RequiredSubPermission, hasOpusILLicense || hasGoogleLicense, permissionJoinType);
                PermissionChecker<RMPermissionExtensionMasks> opusILExtensionPermissionChecker = new(RequiredPermissionExtention, hasOpusILLicense || hasGoogleLicense, permissionJoinType);
                PermissionChecker<RMSOPermissionMasks> opusSOPermissionChecker = new(RequiredSOPermission, hasOpusSOLicense || hasGoogleLicense, permissionJoinType);
                PermissionChecker<RMDiscoveryPermissionMasks> opusDiscoveryPermissionChecker = new(RequiredDiscoveryPermission, hasDiscoveryLicense || hasSalesforceDiscoveryLicense, permissionJoinType);
                PermissionChecker<RMDiscoverySalesforcePermissionMask> opusSalesforceDiscoveryPermissionChecker = new(RequiredSalesforceDiscoveryPermission, hasSalesforceDiscoveryLicense, permissionJoinType);
                PermissionChecker<RMDiscoveryGoogleROTPermissionMask> opusGoogleROTDiscoveryPermissionChecker = new(RequiredGoogleROTDiscoveryPermission, hasGoogleROTLicense, permissionJoinType);
                PermissionChecker<RMDiscoveryFileSystemPermissionMask> opusFSDiscoveryPermissionChecker = new(RequiredFSDiscoveryPermission, hasFSDiscoveryLicense, permissionJoinType);
                PermissionChecker<RMReportPermissionMasks> opusReportPermissionChecker = new(RequiredReportPermission, hasOpusILLicense || hasGoogleLicense, permissionJoinType);

                if (DiffPermissionJoinType == PermissionJoinType.Any)
                {
                    if (!opusILPermissionChecker.IsNonePermission && await opusILPermissionChecker.CheckPermissionAsync())
                    {
                        return true;
                    }
                    if (!opusSOPermissionChecker.IsNonePermission && await opusSOPermissionChecker.CheckPermissionAsync())
                    {
                        return true;
                    }
                    
                    if (!opusILSubPermissionChecker.IsNonePermission && opusILSubPermissionChecker.LicenseEnable)
                    {
                        if (RMPermissionMasks.PhysicalEndUser == RequiredPermission && !(await trimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.PhysicalAdmin)))
                        {
                            //phy end user
                            if (await opusILSubPermissionChecker.CheckPermissionAsync())
                            {
                                return true;
                            }
                        }

                        //if (RMPermissionExtensionMasks.RestoreCenterAccess == RequiredPermissionExtention && !(await trimmingHelper.DoesUserHasThisPermissionAsync(RMSOPermissionMasks.ContentRepositoyEnduser)))
                        //{
                        //    //restore center control
                        //    if (await opusILSubPermissionChecker.CheckPermissionAsync())
                        //    {
                        //        return true;
                        //    }
                        //}
                    }

                    if (!opusDiscoveryPermissionChecker.IsNonePermission && await opusDiscoveryPermissionChecker.CheckPermissionAsync())
                    {
                        return true;
                    }

                    if (!opusFSDiscoveryPermissionChecker.IsNonePermission && await opusFSDiscoveryPermissionChecker.CheckPermissionAsync())
                    {
                        return true;
                    }

                    if (!opusSalesforceDiscoveryPermissionChecker.IsNonePermission && await opusSalesforceDiscoveryPermissionChecker.CheckPermissionAsync())
                    {
                        return true;
                    }
                    if (!opusGoogleROTDiscoveryPermissionChecker.IsNonePermission && await opusGoogleROTDiscoveryPermissionChecker.CheckPermissionAsync())
                    {
                        return true;
                    }

                    if (!opusILExtensionPermissionChecker.IsNonePermission && await opusILExtensionPermissionChecker.CheckPermissionAsync())
                    {
                        return true;
                    }
                    if (!opusReportPermissionChecker.IsNonePermission && await opusReportPermissionChecker.CheckPermissionAsync())
                    {
                        return true;
                    }
                    return false;
                }
                else 
                {
                    if (opusILPermissionChecker.LicenseEnable && !opusILPermissionChecker.IsNonePermission && !await opusILPermissionChecker.CheckPermissionAsync())
                    {
                        logger.Warn($"user do not have opusIL permission to access control:{reqUrl}");
                        return false;
                    }
                    if (opusSOPermissionChecker.LicenseEnable && !opusSOPermissionChecker.IsNonePermission && !await opusSOPermissionChecker.CheckPermissionAsync())
                    {
                        logger.Warn($"user do not have opusSO permission extention to access control:{reqUrl}");
                        return false;
                    }

                    if (!opusILSubPermissionChecker.IsNonePermission && opusILSubPermissionChecker.LicenseEnable)
                    {
                        if (RMPermissionMasks.PhysicalEndUser == RequiredPermission && !(await trimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.PhysicalAdmin)))
                        {
                            //phy end user
                            if (!await opusILSubPermissionChecker.CheckPermissionAsync())
                            {
                                logger.Warn($"user do not have sub permission extention to access control:{reqUrl}");
                                return false;
                            }
                        }
                    }

                    if (opusILExtensionPermissionChecker.LicenseEnable && !opusILExtensionPermissionChecker.IsNonePermission && !await opusILExtensionPermissionChecker.CheckPermissionAsync())
                    {
                        logger.Warn($"user do not have permission extention to access control:{reqUrl}");
                        return false;
                    }
                    if (opusReportPermissionChecker.LicenseEnable && !opusReportPermissionChecker.IsNonePermission && !await opusReportPermissionChecker.CheckPermissionAsync())
                    {
                        logger.Warn($"user do not have opusReport permission to access control:{reqUrl}");
                        return false;
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Api AUthorize Failed:{reqUrl}, {ex.ToString()}");
            }

            return false;
        }

        private async Task<bool> IsMultiGeoIpAllowedAsync(AuthorizationFilterContext filterContext, RMIdentity identity)
        {
            var actionDescriptor = filterContext.ActionDescriptor as ControllerActionDescriptor;
            if (actionDescriptor == null || !typeof(BaseApiController).IsAssignableFrom(actionDescriptor.ControllerTypeInfo.AsType()))
            {
                return true;
            }

            bool isEnableMultiGeo = identity.IsEnableMultiGeo;
            if (!isEnableMultiGeo)
            {
                return true;
            }

            string clientIP = ClientRequestLocalValue.ClientIP;
            if (string.IsNullOrEmpty(clientIP))
            {
                clientIP = filterContext.HttpContext.GetClientIP();
            }

            string dataCenter = RMGlobalConfiguration.AppConfig[RMAppSettingKey.AOS_DATA_CENTER];
            if (await MultiGeoSettingService.ValidateLoginIPAsync(clientIP, dataCenter))
            {
                return true;
            }

            logger.Warn($"The login IP is not allowed to access data center [{dataCenter}]. Reject the request.");
            filterContext.Result = new ObjectResult("Current Ip is blocked for this data center") { StatusCode = (int)HttpStatusCode.Forbidden };
            return false;
        }
    }
}
