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
using AngleSharp.Common;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Multi_Geo;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.ArchiverRestore;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.Service.Security;
using AvePoint.RA.Service.Services.ControlPanel;
using AvePoint.RA.Service.Services.Discovery.FileSystem.License;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Multi_Geo;

namespace AvePoint.RA.Web.Models.Resource
{
    public static class GResources
    {
        public static readonly string RouterUrl_Root = "Root";
        private readonly static RALogger _logger = RALogger.GetInstance(typeof(GResources));
        private static List<ResourceItem> _items = null;
        private static readonly object _itemsLock = new object();
        private const string S_SALESFORCEDISCOVERYLICENSE = "SALESFORCE_DISCOVERY_LICENSE";
        private const string S_OPUSDISCOVERYLICENSE = "OPUS_DISCOVERY_LICENSE";
        private static IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private static readonly ITenantService TenantService = PlatformWindsorManager.GetService<ITenantService>();
        private static IRMSecurityTrimmingHelper SecurityTrimmingHelper => PlatformWindsorManager.GetService<IRMSecurityTrimmingHelper>();
        private static readonly IRestoreSearchService _restoreSearchService = PlatformWindsorManager.GetService<IRestoreSearchService>();
        private static readonly IMultiGeoDataCenterService _multiGeoDataCenterService = PlatformWindsorManager.GetService<IMultiGeoDataCenterService>();
        //private static readonly List<ResourceKeys> mArchiverSkipKeys = new List<ResourceKeys>()
        //{
        //    ResourceKeys.BCM_GlobalSearch,
        //    ResourceKeys.BCM_HybridSearch,
        //    ResourceKeys.DC,
        //    ResourceKeys.DC_Download,
        //    ResourceKeys.PRM_ManageHold,
        //    ResourceKeys.PRM_GlobalSearch,
        //    ResourceKeys.RelatedRecords,
        //    ResourceKeys.RDM_MAProcessesManagement,            
        //    ResourceKeys.Connector,
        //    ResourceKeys.Connector_CreateOrEdit,
        //    ResourceKeys.Connector_Index,
        //    ResourceKeys.CP_CSDApiKeyManagement,
        //    ResourceKeys.BCM_ManageHold,
        //    ResourceKeys.CP_AgentManagement,
        //    ResourceKeys.CP_AccountManagement,
        //    ResourceKeys.CP_EditEmailTemplate,
        //    ResourceKeys.CP_EmailTemplate,
        //    ResourceKeys.BCM_ContentRepositoryManagement_UniqueId,
        //    ResourceKeys.BCM_ContentRepositoryManagement_Import,
        //    ResourceKeys.BCM_ContentRepositoryManagement_Classification,
        //    ResourceKeys.RDM_ViewWorkFlow,
        //    ResourceKeys.RDM_CreateWorkFlow,
        //};
        public static List<ResourceItem> Items 
        {
            get 
            {
                if (_items == null) 
                {
                    lock (_itemsLock)
                    {
                        if (_items == null)
                        {
                            var items = new List<ResourceItem>();
                            items.AddRange(new CommonResource().Get());
                            items.AddRange(new RDMResource().Get());
                            items.AddRange(new BCMResource().Get());
                            items.AddRange(new ReportCenterResource().Get());
                            items.AddRange(new PhysicalResource().Get());
                            items.AddRange(new JobMonitorResource().Get());
                            items.AddRange(new ControlPanelResource().Get());
                            items.AddRange(new DownloadCenterResource().Get());
                            items.AddRange(new RelatedRecordResource().Get());
                            items.AddRange(new CustomizeConnectorResource().Get());
                            items.AddRange(new ArchiverResource().Get());
                            items.AddRange(new MTResource().Get());
                            items.AddRange(new MLResource().Get());
                            _items = items;
                        }
                    }
                }
                return _items.Concat(new AnalysisResource().Get()).ToList();
            }
             
        }

        private static List<ResourceKeys> _notDisplayForOnlyGoogle = null;

        public static List<ResourceKeys> NotDisplayForOnlyGoogle
        {
            get
            {
                if (_notDisplayForOnlyGoogle == null)
                {
                    _notDisplayForOnlyGoogle = new List<ResourceKeys>()
                    {
                        ResourceKeys.CP_StorageSettings,
                        ResourceKeys.CP_ExportSettings,
                        ResourceKeys.CP_StubSettings,
                        ResourceKeys.CP_EndUserRestoreSettings,
                        ResourceKeys.RC_ActionAuditReport_Management,
                        ResourceKeys.RC_ActionAuditReport_Profile,
                        ResourceKeys.RC_ActionAuditReport_ShowReport,
                        ResourceKeys.RC_ActionAuditReport_ViewDetail,
                        ResourceKeys.RC_ActionAuditReport_Create,
                        ResourceKeys.RC_ActionAuditReport_Edit,
                    };
                }
                return _notDisplayForOnlyGoogle;
            } 
        }

        public static List<ResourceKeys> NonFSContentSourceResourceKeys = new List<ResourceKeys>()
        {
            // SPO content sources
            ResourceKeys.BCM_ContentSourcesForSharePointOnline,

            // EXO content sources
            ResourceKeys.BCM_ContentSourcesForExchangeOnline,

            // OneDrive content sources
            ResourceKeys.BCM_ContentSourcesForOneDriveforBusiness,

            // SharePoint On-Premises content sources
            ResourceKeys.BCM_ContentSourcesForSharePointOnPremises,

            // Physical Records content sources
            ResourceKeys.BCM_ContentSourcesForPhysicalRecords,

            // Teams content sources
            ResourceKeys.BCM_ContentSourcesForTeams,
            ResourceKeys.BCM_ContentSourcesForTeams_Switch,

            // Box content sources
            ResourceKeys.BCM_ContentSourcesForBox,
            ResourceKeys.BCM_BoxConfigureConnection,

            // Google content sources
            ResourceKeys.BCM_ContentSourcesForGoogle,
            ResourceKeys.BCM_GoogleConfigureConnection,

            // Azure Files
            ResourceKeys.BCM_ContentSourcesForAzureFiles,
            ResourceKeys.BCM_AzFileShareConfigureConnection,

            //action Audit
            ResourceKeys.RC_ActionAuditReport_Management,
            ResourceKeys.RC_ActionAuditReport_Profile,
            ResourceKeys.RC_ActionAuditReport_ShowReport,
            ResourceKeys.RC_ActionAuditReport_ViewDetail,
            ResourceKeys.RC_ActionAuditReport_Create,
            ResourceKeys.RC_ActionAuditReport_Edit,

            //restore
            ResourceKeys.RC_RestoreReport_Management,
            ResourceKeys.RC_RestoreReport_Profile,
            ResourceKeys.RC_RestoreReport_ShowReport,
            ResourceKeys.RC_RestoreReport_ViewDetail,
            ResourceKeys.RC_RestoreReport_Create,
            ResourceKeys.RC_RestoreReport_Edit,

            //AvailableSpace
            ResourceKeys.RC_AvailableSpaceReport_Management,
            ResourceKeys.RC_AvailableSpaceReport_Profile,
            ResourceKeys.RC_AvailableSpaceReport_ShowReport,
            ResourceKeys.RC_AvailableSpaceReport_ViewDetail,

            //Archived sites
            ResourceKeys.RC_StorageOptimizationReport_Management
        }
        .Concat(new MTResource().Get().Select(i => i.Key))
        .Concat(new MLResource().Get().Select(i => i.Key))
        .Concat(new PhysicalResource().Get().Select(i => i.Key))
        .Concat(new AnalysisResource().Get().Select(i => i.Key)).ToList();

        public async static Task<List<ResourceItem>> GetResourceViaPermission(
            RMPermissionMasks permissionMasks,
            RMSOPermissionMasks soPermissionMasks,
            RMDiscoveryPermissionMasks discoveryPermissionMasks,
            RMReportPermissionMasks reportingPermissionMasks,
            RMPermissionExtensionMasks permissionExtentionMasks = RMPermissionExtensionMasks.None,
            RMDiscoverySalesforcePermissionMask salesforceDiscoveryPermissionMasks = RMDiscoverySalesforcePermissionMask.None,
            RMDiscoveryGoogleROTPermissionMask googleROTPermissionMasks = RMDiscoveryGoogleROTPermissionMask.None,
            RMDiscoveryFileSystemPermissionMask fsDiscoveryPermissionMasks = RMDiscoveryFileSystemPermissionMask.None,
            bool isAdmin = false)
        {
            var tenantService = PlatformWindsorManager.GetService<ITenantService>();
            var licenseHelperService = PlatformWindsorManager.GetService<ILicenseHelperService>();
            var isCSDTenant = tenantService.IsCSDTenant();
            var RMJobService = PlatformWindsorManager.GetService<IRMJobService>();
            var allItems = Items;
            IEnumerable<ResourceItem> data;
            //当前判断Lifecycle 相关resource
            if (isAdmin)
            {
                data = allItems.Where(r => (r.Permission & permissionMasks) == r.Permission && r.Permission != RMPermissionMasks.None && (isCSDTenant || !r.IsCSDResource));
            }
            else
            {
                data = allItems.Where(r => (((r.Permission & permissionMasks) == r.Permission && r.Permission != RMPermissionMasks.None) || r.ReportPermission != RMReportPermissionMasks.None) && (isCSDTenant || !r.IsCSDResource));
            }
            var archiverData = allItems.Where(r => (r.SOPermission & soPermissionMasks) == r.SOPermission && r.SOPermission != RMSOPermissionMasks.None).ToList();
            var discoveryData = allItems.Where(r => ((r.DiscoveryPermission & discoveryPermissionMasks) == r.DiscoveryPermission) && r.DiscoveryPermission != RMDiscoveryPermissionMasks.None).ToList();
            var salesforceDiscoveryData = allItems.Where(r => ((r.SalesforceDiscoveryPermission & salesforceDiscoveryPermissionMasks) == r.SalesforceDiscoveryPermission) && r.SalesforceDiscoveryPermission != RMDiscoverySalesforcePermissionMask.None).ToList();
            var googleROTDiscoveryData = allItems.Where(r => ((r.GoogleROTDiscoveryPermission & googleROTPermissionMasks) == r.GoogleROTDiscoveryPermission) && r.GoogleROTDiscoveryPermission != RMDiscoveryGoogleROTPermissionMask.None).ToList();
            var fsDiscoveryData = allItems.Where(r => ((r.FSDiscoveryPermission & fsDiscoveryPermissionMasks) == r.FSDiscoveryPermission) && r.FSDiscoveryPermission != RMDiscoveryFileSystemPermissionMask.None).ToList();

            var hasFSDiscoveryLicense = RMDiscoveryFSLicenseHelper.HasDiscoveryFileSystemLicense();

            var hasOpusSOLicense = licenseHelperService.HasOpusSOLicense;
            var hasOpusLifecycleLicense = licenseHelperService.HasOpusILLicense;
            var hasOpusDiscoveryLicense = licenseHelperService.HasOpusDiscoveryLicense;
            var hasOpusGoogleLicense = licenseHelperService.HasOpusGoogleLicense;
            var hasSalesforceLicense = licenseHelperService.HasOpusSalesforceDiscoveryLicense;
            var hasGoogleROTLicense = licenseHelperService.HasOpusGoogleROTDiscoveryLicense;
            // Note: hasFSDiscoveryLicense is computed above using the full license helper.
            var hasEnabledJPMCFileSystemFeature = await RMKeyValueDao.GetValueByKeyAsync<bool>(KeyNameCollection.EnableJPMCFileSystemFeature, false);
            List<(bool, IEnumerable<ResourceItem>)> discoveryLicenseList = [
                (hasOpusDiscoveryLicense, discoveryData),
                (hasSalesforceLicense, salesforceDiscoveryData),
                (hasGoogleROTLicense, googleROTDiscoveryData),
                (hasFSDiscoveryLicense, fsDiscoveryData)
            ];
            if (!hasOpusLifecycleLicense && !hasOpusSOLicense && hasOpusGoogleLicense) 
            {
                data = data.Where(r => !NotDisplayForOnlyGoogle.Contains(r.Key));
            }

            if(!hasOpusLifecycleLicense)
            {
                data = data.Where(r => r.Key != ResourceKeys.RECO_ContentSource_Tab);
            }

            if (!hasEnabledJPMCFileSystemFeature)
            {
                var targetResourceKeys = new List<ResourceKeys>()
                {
                    ResourceKeys.BCM_FSConnectionMonitor,
                    ResourceKeys.BCM_FSConnectionDetail,
                };
                data = data.Where(r => !targetResourceKeys.Contains(r.Key));
            }

            if (permissionMasks == RACommonUtility.Permission.PermissionWrappers.StandardUser)
            {
                data = data.Where(r => (r.Key != ResourceKeys.PRM_ManageHold));
            }
            if (permissionMasks == RACommonUtility.Permission.PermissionWrappers.HoldManagerUser)
            {
                data = data.Where(r => (r.Key != ResourceKeys.Home));
            }
            if (permissionMasks == RACommonUtility.Permission.PermissionWrappers.ReviewUser)
            {
                data = data.Where(r => (r.Key != ResourceKeys.BCM_HybridSearch));
            }

            if (!await licenseHelperService.IsEnableMaestroAI() && !RMKeyValueDao.EnableZeroShotFeature())
            {
                data = data.Where(r => r.Key != ResourceKeys.ML_MachineLearning && r.Key != ResourceKeys.MT_MachineLearningReview);
            }

            if (permissionExtentionMasks != RMPermissionExtensionMasks.None)
            {
                if (!RMJobService.RunDisposalInRecords())
                {
                    //Revoke Teams for base on dao account
                    permissionExtentionMasks &= ~RMPermissionExtensionMasks.TeamsAdmin;
                }
                var dataExtension = allItems.Where(r => (r.PermissionExtension & permissionExtentionMasks) == r.PermissionExtension && r.PermissionExtension != RMPermissionExtensionMasks.None && (isCSDTenant || !r.IsCSDResource));
                // For temp solution, non- super admin users, hide storage optimization management resource
                //TODO: remove this and refactor Google End-user then
                if (!isAdmin)
                {
                    dataExtension =
                        dataExtension.Where(r => r.Key != ResourceKeys.RC_StorageOptimizationReport_Management);
                }
                data = data.Concat(dataExtension);
            }

                var discoveryLicenses = discoveryLicenseList
                    .Where(discoveryLicense => discoveryLicense.Item1).ToArray();
            var discoveryItems = discoveryLicenses.SelectMany(discoveryLicense => discoveryLicense.Item2);

            if (hasOpusSOLicense)
            {
                //如果当前存在SO license，将so相关resource 添加
                data = data.Concat(archiverData);

                //如果当前存在Discovery license，将Discovery相关resource添加
                
                data = data.Concat(discoveryItems);
            }
            else
            {
                bool isNewOpusTenantWithoutGoogle = tenantService.IsNewOpusTenant() && !hasOpusGoogleLicense;
                /*if (hasOpusDiscoveryLicense)
                {
                    if (isNewOpusTenantWithoutGoogle && !hasSalesforceLicense)
                    {
                        //如果是New Opus且当前不存在SO license但存在Discovery license，仅显示Discovery相关resource
                        data = discoveryData;
                    }
                    else
                    {
                        //如果不是New Opus且当前不存在SO license但存在Discovery license，显示Lifecycle + Discovery resource
                        data = data.Concat(discoveryData);
                    }
                }     */
                data = discoveryLicenses.Length switch
                {
                    1 when isNewOpusTenantWithoutGoogle => discoveryLicenses[0].Item2,
                    >= 1 => data.Concat(discoveryItems),
                    _ => data
                };
            }

            if (!RMJobService.RunDisposalInRecords())
            {
                data = data.Where(r => r.Key != ResourceKeys.Archiver_RestoreCenter && r.Key != ResourceKeys.CP_EndUserRestoreSettings && r.Key != ResourceKeys.CP_ExportSettings_CompliantExports);
            }
            if (!RMKeyValueDao.EnableTeamsFeature())
            {
                data = data.Where(r => r.Key != ResourceKeys.Source_Teams && r.Key != ResourceKeys.BCM_ContentSourcesForTeams && r.Key != ResourceKeys.BCM_ContentSourcesForTeams_Switch);
            }
            //Check export index permission 
            if(IsExportIndexPermission())
            {
                data = data.Concat(new List<ResourceItem>
                {
                    new ResourceItem()
                    {
                        Key = ResourceKeys.Archiver_Export_Index,
                        Value = ResourceKeys.Archiver_Export_Index.ToString(),
                    }
                });
            }

            if(IsDiscoveryPlanPermission() && HasOffice365DiscoveryLicense())
            {
                data = data.Concat(new List<ResourceItem>
                {
                    new ResourceItem()
                    {
                        Key = ResourceKeys.FileAnalysis_PlanProfile,
                        Value = ResourceKeys.FileAnalysis_PlanProfile.ToUrl(RouterUrl_Root),
                        DiscoveryPermission = RMDiscoveryPermissionMasks.AccessAll
                    }
                });
            }

            //Check restore discovery permisson
            if (_restoreSearchService.IsEnableFullTextIndexSearch() && 
                ((soPermissionMasks & RMSOPermissionMasks.RestoreCenterSearch) == RMSOPermissionMasks.RestoreCenterSearch
                || ((soPermissionMasks & RMSOPermissionMasks.CommonModuleAccess) == RMSOPermissionMasks.CommonModuleAccess 
                || (permissionMasks & RMPermissionMasks.ContentRepositoyEnduser) == RMPermissionMasks.ContentRepositoyEnduser) 
                && ((permissionMasks & RMPermissionMasks.SPOEnduser) == RMPermissionMasks.SPOEnduser || (permissionMasks & RMPermissionMasks.SPOAdmin) == RMPermissionMasks.SPOAdmin)
                || (permissionMasks & RMPermissionMasks.OneDriveEnduser) == RMPermissionMasks.OneDriveEnduser || (permissionMasks & RMPermissionMasks.OneDriveAdmin) == RMPermissionMasks.OneDriveAdmin)
                )
            {
                data = data.Concat(new List<ResourceItem>
                {
                    new ResourceItem()
                    {
                        Key = ResourceKeys.Archiver_RestoreCenter_Discovery,
                        Value = ResourceKeys.Archiver_RestoreCenter_Discovery.ToString(),
                    }
                });
            }

            if (!RMKeyValueDao.IsSupportMultipleGeoFeature() || !_multiGeoDataCenterService.IsMainDC())
            {
                data = data.Where(r => r.Key != ResourceKeys.CP_Multi_GEOSettings);
            }

            if (!IsTrailLicence())
            {
                if (licenseHelperService.HasOpusSOLicense)
                {
                    var tempData = data.ToList();
                    tempData.Add(new ResourceItem()
                    {
                        Key = ResourceKeys.Archiver_Discovery_Optimization_RunJob,
                        Value = ResourceKeys.Archiver_Discovery_Optimization_RunJob.ToString(),
                        SOPermission = RMSOPermissionMasks.ControlPanelAdmin
                    });
                    return tempData;
                }
                _logger.Warn("not trail licence but has no so licence");
            }
            return data.ToList();
        }

        public async static Task<List<ResourceItem>> GetResourceViaPermissionOfMultiGeoAsync(RMPermissionMasks permissionMasks, RMSOPermissionMasks soPermissionMasks, RMDiscoveryPermissionMasks discoveryPermissionMasks, RMReportPermissionMasks reportingPermissionMasks, RMPermissionExtensionMasks permissionExtentionMasks = RMPermissionExtensionMasks.None, RMDiscoverySalesforcePermissionMask salesforceDiscoveryPermissionMasks = RMDiscoverySalesforcePermissionMask.None, RMDiscoveryGoogleROTPermissionMask googleROTPermissionMasks = RMDiscoveryGoogleROTPermissionMask.None, RMDiscoveryFileSystemPermissionMask fsDiscoveryPermissionMasks = RMDiscoveryFileSystemPermissionMask.None, bool isAdmin = false)
        {
            var allItems = await GetResourceViaPermission(permissionMasks, soPermissionMasks, discoveryPermissionMasks, reportingPermissionMasks, permissionExtentionMasks, salesforceDiscoveryPermissionMasks, googleROTPermissionMasks, fsDiscoveryPermissionMasks, isAdmin);
            
            if (_multiGeoDataCenterService.IsMainDC())
            {
                return allItems;
            }
            return allItems.Where(item => !NonFSContentSourceResourceKeys.Contains(item.Key)).ToList();
        }

        public static string ToUrl(this ResourceKeys resourceKey, string prefix = "")
        {
            string baseUrl = resourceKey.ToString().Replace('_', '/');
            return string.IsNullOrEmpty(prefix) ? $"/{baseUrl}" : $"/{prefix}/{baseUrl}";
        }
        private static bool IsTrailLicence()
        {
            try
            {
                var client = AosApiUtility.GetAosModernClient(TenantLocalValue.LogonGroupId);
                var info = client.LicenseService.GetLicenseAsync(RecordsConstants.RECORDS_APPLICATION_NAME).GetAwaiter().GetResult();
                _logger.Info($"current TenantLocalValue.LogonGroupId is {TenantLocalValue.LogonGroupId}");
                if (info.Type == Cloud.Sdk.Data.AosModern.LicenseType.Trial)
                {
                    _logger.Info($"IsTrailLicence:this licence type is Trail licence");
                    return true;
                }
                else
                {
                    _logger.Info($"IsTrailLicence:this licence type not Trail licence,type:{info.Type}");
                    return false;
                }
            }
            catch (Exception e)
            {
                _logger.Error($"some thing went wrong when check is trail licence ,error :{e.ToString()}");
                return false;
            }
        }

        private static bool IsExportIndexPermission()
        {
            try
            {
                if (long.TryParse(RMKeyValueDao.GetValueByKey(RMKeyValuesConstants.PreviewFeature)?.Value, out var module))
                {
                    if (((PreviewFeature)module & PreviewFeature.ExportIndex) == PreviewFeature.ExportIndex)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch(Exception e)
            {
                _logger.Error($"some thing went wrong when check is export index permission ,error :{e.ToString()}");
                return false;
            }
        }

        private static bool HasOffice365DiscoveryLicense()
        {
            return SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMDiscoveryPermissionMasks.AccessAll).GetAwaiter().GetResult();
        }

        private static bool IsDiscoveryPlanPermission()
        {
            try
            {
                if (long.TryParse(RMKeyValueDao.GetValueByKey(RMKeyValuesConstants.PreviewFeature)?.Value, out var module))
                {
                    if (((PreviewFeature)module & PreviewFeature.DiscoveryPlan) == PreviewFeature.DiscoveryPlan)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch(Exception e)
            {
                _logger.Error($"some thing went wrong when check is export index permission ,error :{e.ToString()}");
                return false;
            }
        }
    }
}
