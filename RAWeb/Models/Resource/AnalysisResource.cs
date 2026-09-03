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
using Aspose.Pdf;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.Service.Services.Discovery.FileSystem.License;
using AvePoint.RA.Service.Services.Discovery.Google.License;
using AvePoint.Wrapper.Common;
using System.Collections.Generic;

namespace AvePoint.RA.Web.Models.Resource
{
    public class AnalysisResource : BaseResource
    {
        private IRMSecurityTrimmingHelper SecurityTrimmingHelper => PlatformWindsorManager.GetService<IRMSecurityTrimmingHelper>();

        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();

        public List<ResourceItem> GetResource()
        {
            var res = new List<ResourceItem>();

            if (OneOfDataSourceHasLicense())
            {
                res.Add(new ResourceItem()
                {
                    Key = ResourceKeys.FileAnalysis_Discovery,
                    Value = ResourceKeys.FileAnalysis_Discovery.ToUrl(RouterUrl_Root),
                });
                res.Add(new ResourceItem()
                {
                    Key = ResourceKeys.FileAnalysis_Discovery_Configuration,
                    Value = ResourceKeys.FileAnalysis_Discovery_Configuration.ToUrl(RouterUrl_Root),
                });
                res.Add(new ResourceItem()
                {
                    Key = ResourceKeys.FileAnalysis_Discovery_RunJob,
                    Value = ResourceKeys.FileAnalysis_Discovery_RunJob.ToUrl(RouterUrl_Root),
                });
                res.Add(new ResourceItem()
                {
                    Key = ResourceKeys.FileAnalysis_Discovery_Finish,
                    Value = ResourceKeys.FileAnalysis_Discovery_Finish.ToUrl(RouterUrl_Root),
                });
                res.Add(new ResourceItem()
                {
                    Key = ResourceKeys.FileAnalysis_InactiveOptimization,
                    Value = ResourceKeys.FileAnalysis_InactiveOptimization.ToUrl(RouterUrl_Root),
                });
            }

            if(HasOffice365DiscoveryLicense())
            {
                res.ForEach(item => item.DiscoveryPermission = RMDiscoveryPermissionMasks.AccessAll);
            }

            if(HasSalesForceDiscoveryLicense())
            {
                res.ForEach(item => item.SalesforceDiscoveryPermission = RMDiscoverySalesforcePermissionMask.AccessAll);
            }

            if (HasGoogleROTDiscoveryLicense())
            {
                res.ForEach(item => item.GoogleROTDiscoveryPermission = RMDiscoveryGoogleROTPermissionMask.AccessAll);
            }

            if (HasFileSystemDiscoveryLicense())
            {
                res.ForEach(item =>
                {
                    item.FSDiscoveryPermission = RMDiscoveryFileSystemPermissionMask.AccessAll;
                    item.Permission = RMPermissionMasks.FSAdmin;
                });
            }

            if (HasOffice365DiscoveryLicense() || HasGoogleROTDiscoveryLicense() || HasFileSystemDiscoveryLicense())
            {
                var rotResourceItem = new ResourceItem()
                {
                    Key = ResourceKeys.FileAnalysis_ROTOptimization,
                    Value = ResourceKeys.FileAnalysis_ROTOptimization.ToUrl(RouterUrl_Root),
                };
                
                if (HasGoogleROTDiscoveryLicense())
                {
                    rotResourceItem.GoogleROTDiscoveryPermission = RMDiscoveryGoogleROTPermissionMask.AccessAll;
                }

                if(HasOffice365DiscoveryLicense())
                {
                    rotResourceItem.DiscoveryPermission = RMDiscoveryPermissionMasks.AccessAll;
                }

                if (HasFileSystemDiscoveryLicense())
                {
                    rotResourceItem.FSDiscoveryPermission = RMDiscoveryFileSystemPermissionMask.AccessAll;
                    rotResourceItem.Permission = RMPermissionMasks.FSAdmin;
                }

                res.Add(rotResourceItem);
            }

            if (HasOffice365DiscoveryLicense())
            {
                res.Add(new ResourceItem()
                {
                    Key = ResourceKeys.FileAnalysis_PlanView,
                    Value = ResourceKeys.FileAnalysis_PlanView.ToUrl(RouterUrl_Root),
                    DiscoveryPermission = RMDiscoveryPermissionMasks.AccessAll
                });
            }

            if(HasOffice365DiscoveryLicense())
            {
                res.Add(new ResourceItem()
                {
                    Key = ResourceKeys.FileAnalysis_Progress,
                    Value = ResourceKeys.FileAnalysis_Progress.ToUrl(RouterUrl_Root),
                    DiscoveryPermission = RMDiscoveryPermissionMasks.AccessAll
                });
            }

            if (HasFileSystemDiscoveryLicense())
            {
                res.Add(new ResourceItem()
                {
                    Key = ResourceKeys.FileAnalysis_Discovery_ConfigurationFSConfigConnection,
                    Value = ResourceKeys.FileAnalysis_Discovery_ConfigurationFSConfigConnection.ToUrl(RouterUrl_Root),
                    FSDiscoveryPermission = RMDiscoveryFileSystemPermissionMask.AccessAll,
                    Permission = RMPermissionMasks.FSAdmin
                });
            }

            return res;
        }

        private bool HasOffice365DiscoveryLicense()
        {
            return SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMDiscoveryPermissionMasks.AccessAll).GetAwaiter().GetResult();
        }

        private bool HasSalesForceDiscoveryLicense()
        {
            return TenantService.CheckLicenseWithAdditionalProduct(TenantLocalValue.LogonGroupId, PaidForProduct.OpusSalesforceDiscovery);
        }

        private bool HasGoogleROTDiscoveryLicense()
        {
            return TenantService.CheckLicenseWithAdditionalProduct(TenantLocalValue.LogonGroupId, PaidForProduct.OpusGoogleWorkspaceDiscovery);
        }

        private bool HasFileSystemDiscoveryLicense()
        {
            return RMDiscoveryFSLicenseHelper.HasDiscoveryFileSystemLicense();
        }

        private bool OneOfDataSourceHasLicense()
        {
            return HasOffice365DiscoveryLicense() || HasSalesForceDiscoveryLicense() || HasGoogleROTDiscoveryLicense() || HasFileSystemDiscoveryLicense();
        }

        public override List<ResourceItem> Get()
        {
            return GetResource();
        }
    }
}
