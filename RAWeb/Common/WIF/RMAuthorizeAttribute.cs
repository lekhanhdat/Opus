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
using AvePoint.RA.Contract.RMWeb.Account.Security;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.DB.SecurityTrimming.Model;
using AvePoint.RA.Service.Security;
using AvePoint.RA.Service.Services.ControlPanel;
using AvePoint.RA.Web.Extentions.Util;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Common.WIF
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    internal sealed class RMAuthorizeAttribute : BaseAuthorizeAttribute
    {
        private static RALogger Logger = RALogger.GetInstance(typeof(RMAuthorizeAttribute));
        public RMPermissionMasks RequiredPermission { get; private set; }
        public RMSOPermissionMasks RMSOPermission { get; private set; }
        public RMDiscoveryPermissionMasks RMDiscoveryPermission { get; private set; }
        public RMDiscoverySalesforcePermissionMask RMSalesforceDiscoveryPermission { get; private set; }
        public RMDiscoveryGoogleROTPermissionMask RMGoogleROTDiscoveryPermission { get; private set; }
        public RMDiscoveryFileSystemPermissionMask RMDiscoveryFileSystemPermission { get; private set; }
        public PermissionJoinType permissionJoinType { get; set; } = PermissionJoinType.And;
        public PermissionJoinType DiffPermissionJoinType { get; set; } = PermissionJoinType.Any;
        public RMPermissionExtensionMasks RequiredPermissionExtention { get; private set; }
        public bool Preferred { get; set; }

        public ILicenseHelperService LicenseHelper => PlatformWindsorManager.GetService<ILicenseHelperService>();
        //private ITenantInfoDao TenantInfoDao => PlatformWindsorManager.GetService<ITenantInfoDao>();
        public RMAuthorizeAttribute(RMPermissionMasks permission, bool preferred = true)
        {
            RequiredPermission = permission;
            Preferred = preferred;
        }

        public RMAuthorizeAttribute(RMPermissionMasks permission, RMSOPermissionMasks mSOPermissionMasks, PermissionJoinType permissionJoinType = PermissionJoinType.Any, bool preferred = true)
        {
            RequiredPermission = permission;
            RMSOPermission = mSOPermissionMasks;
            this.permissionJoinType = permissionJoinType;
            Preferred = preferred;
        }

        public RMAuthorizeAttribute(RMPermissionMasks permission, RMSOPermissionMasks mSOPermissionMasks, RMDiscoveryPermissionMasks discoveryPermissionMasks, RMDiscoverySalesforcePermissionMask discoverySalesforcePermissionMask, PermissionJoinType permissionJoinType = PermissionJoinType.Any, bool preferred = true)
        {
            RequiredPermission = permission;
            RMSOPermission = mSOPermissionMasks;
            RMDiscoveryPermission = discoveryPermissionMasks;
            RMSalesforceDiscoveryPermission = discoverySalesforcePermissionMask;
            this.permissionJoinType = permissionJoinType;
            Preferred = preferred;
        }

        public RMAuthorizeAttribute(RMPermissionMasks permission, 
            RMSOPermissionMasks mSOPermissionMasks, 
            RMDiscoveryPermissionMasks discoveryPermissionMasks, 
            RMDiscoverySalesforcePermissionMask discoverySalesforcePermissionMask, 
            RMDiscoveryGoogleROTPermissionMask discoveryGoogleROTPermissionMask,
            RMDiscoveryFileSystemPermissionMask discoveryFileSystemPermissionMask,
            PermissionJoinType permissionJoinType = PermissionJoinType.Any, 
            bool preferred = true)
        {
            RequiredPermission = permission;
            RMSOPermission = mSOPermissionMasks;
            RMDiscoveryPermission = discoveryPermissionMasks;
            RMSalesforceDiscoveryPermission = discoverySalesforcePermissionMask;
            RMGoogleROTDiscoveryPermission = discoveryGoogleROTPermissionMask;
            RMDiscoveryFileSystemPermission = discoveryFileSystemPermissionMask;
            this.permissionJoinType = permissionJoinType;
            Preferred = preferred;
        }

        public RMAuthorizeAttribute(RMPermissionMasks permission, 
            RMSOPermissionMasks mSOPermissionMasks, 
            RMDiscoveryPermissionMasks discoveryPermissionMasks, 
            RMDiscoveryGoogleROTPermissionMask discoveryGoogleROTPermissionMask, 
            RMPermissionExtensionMasks permissionExtension, 
            PermissionJoinType permissionJoinType = PermissionJoinType.Any, 
            bool preferred = true)
        {
            RequiredPermission = permission;
            RMSOPermission = mSOPermissionMasks;
            RMDiscoveryPermission = discoveryPermissionMasks;
            RMGoogleROTDiscoveryPermission = discoveryGoogleROTPermissionMask;
            RequiredPermissionExtention = permissionExtension;
            this.permissionJoinType = permissionJoinType;
            Preferred = preferred;
        }

        protected override async Task<bool> IsAuthorizedAsync(AuthorizationFilterContext filterContext, RMIdentity Identity)
        {
            var httpContext = filterContext.HttpContext;
            try
            {
                var actionDescriptor = filterContext.ActionDescriptor as ControllerActionDescriptor;
                //获取方法上添加的RMApiAuthorizeAttribute
                var methodAttributes = actionDescriptor.MethodInfo?.GetCustomAttributes(typeof(RMAuthorizeAttribute), true);
                AvePoint.GCommon.Utility.ArgumentCheck.NotNull(methodAttributes, nameof(methodAttributes));
                if (methodAttributes.Length > 0 && !this.Preferred)
                {
                    //方法添加的RMApiAuthorizeAttribute默认值都是true
                    //当this.Preferred为false时，说明当前this是Controller的RMApiAuthorizeAttribute
                    //当方法和Controller都加了RMApiAuthorizeAttribute，只验证方法的权限即可
                    return true;
                }

                var reqUrl = httpContext.Request.GetUrl();
                var hasOpusILLicense = LicenseHelper.HasOpusILLicense;
                var hasOpusSOLicense = LicenseHelper.HasOpusSOLicense;
                var hasOpusDiscoveryLicense = LicenseHelper.HasOpusDiscoveryLicense;
                var hasOpusGoogleLicense = LicenseHelper.HasOpusGoogleLicense;
                var hasOpusSalesforceDiscoveryLicense = LicenseHelper.HasOpusSalesforceDiscoveryLicense;
                var hasOpusGoogleROTDiscoveryLicense = LicenseHelper.HasOpusGoogleROTDiscoveryLicense;
                var hasOpusFileSystemDiscoveryLicense = LicenseHelper.HasOpusFileSystemDiscoveryLicense;

                PermissionChecker<RMPermissionMasks> opusILPermissionChecker = new(RequiredPermission, hasOpusILLicense || hasOpusGoogleLicense, permissionJoinType);
                PermissionChecker<RMSOPermissionMasks> opusSOPermissionChecker = new(RMSOPermission, hasOpusSOLicense, permissionJoinType);
                PermissionChecker<RMDiscoveryPermissionMasks> opusDiscoveryPermissionChecker = new(RMDiscoveryPermission, hasOpusDiscoveryLicense);
                PermissionChecker<RMDiscoverySalesforcePermissionMask> opusSalesforceDiscoveryPermissionChecker = new(RMSalesforceDiscoveryPermission, hasOpusSalesforceDiscoveryLicense);
                PermissionChecker<RMPermissionExtensionMasks> opusExtensionPermissionChecker = new(RequiredPermissionExtention, hasOpusILLicense || hasOpusGoogleLicense, permissionJoinType);
                PermissionChecker<RMDiscoveryGoogleROTPermissionMask> opusGoogleROTDiscoveryPermissionChecker = new(RMGoogleROTDiscoveryPermission, hasOpusGoogleROTDiscoveryLicense);
                PermissionChecker<RMDiscoveryFileSystemPermissionMask> opusFileSystemDiscoveryPermissionChecker = new(RMDiscoveryFileSystemPermission, hasOpusFileSystemDiscoveryLicense);

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

                    if(!opusDiscoveryPermissionChecker.IsNonePermission && await opusDiscoveryPermissionChecker.CheckPermissionAsync())
                    {
                        return true;
                    }
                    
                    if(!opusSalesforceDiscoveryPermissionChecker.IsNonePermission && await opusSalesforceDiscoveryPermissionChecker.CheckPermissionAsync())
                    {
                        return true;
                    }

                    if (!opusGoogleROTDiscoveryPermissionChecker.IsNonePermission && await opusGoogleROTDiscoveryPermissionChecker.CheckPermissionAsync())
                    {
                        return true;
                    }

                    if (!opusFileSystemDiscoveryPermissionChecker.IsNonePermission && await opusFileSystemDiscoveryPermissionChecker.CheckPermissionAsync())
                    {
                        return true;
                    }

                    if (!opusExtensionPermissionChecker.IsNonePermission && await opusExtensionPermissionChecker.CheckPermissionAsync())
                    {
                        return true;
                    }

                    return false;
                }
                else
                {
                    if (!opusILPermissionChecker.IsNonePermission && !await opusILPermissionChecker.CheckPermissionAsync())
                    {
                        Logger.Warn($"user do not have opusIL permission to access control:{reqUrl}");
                        return false;
                    }
                    if (!opusSOPermissionChecker.IsNonePermission && !await opusSOPermissionChecker.CheckPermissionAsync())
                    {
                        Logger.Warn($"user do not have opusSO permission extention to access control:{reqUrl}");
                        return false;
                    }
                    return true;
                }
            }
            catch(Exception e)
            {
                Logger.Error($"authorize error:{e.ToString()}");
                return false;
            }
        }
    }

    enum UnauthorizedState
    {
        None = 0,
        AccessDenied,
        NoLogin
    }
}