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
namespace AvePoint.StorageOptimization.Archiver.Service.Impl
{
    using AvePoint.Common.RemoteNode.Impl;
    using AvePoint.GCommon;
    using AvePoint.GCommon.Contract.Server.Common.BackupDataSearch;
    using AvePoint.GCommon.Contract.Server.Common.RemoteNode;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
    using AvePoint.GCommon.Contract.Server.EndUserRestoreSetting;
    using AvePoint.GCommon.Contract.StorageOptimization.Archiver;
    using AvePoint.GCommon.Contract.StorageOptimization.Object;
    using AvePoint.GCommon.Contract.Tree.Object;
    using AvePoint.GCommon.Utility;
    using AvePoint.RA.Common;
    using AvePoint.RA.Common.Configurations;
    using AvePoint.RA.Common.Stub;
    using AvePoint.RA.Contract.Configurations;
    using AvePoint.RA.Contract.CloudService;
    using AvePoint.RA.Contract.JobMonitor;
    using AvePoint.RA.Contract.RMWeb;
    using AvePoint.RA.Contract.RMWeb.ArchiverRestore;
    using AvePoint.RA.Contract.RMWeb.Setting;
    using AvePoint.RA.Contract.RMWeb.StorageDevice;
    using AvePoint.RA.Contract.Tenant;
    using AvePoint.RA.DB.Dao;
    using AvePoint.RA.DB.Dao.Utility;
    using AvePoint.RA.DB.Model;
    using AvePoint.RA.RACommonUtility.Common;
    using AvePoint.RA.Service.Services;
    using AvePoint.RA.Service.Services.Settings;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;
    using static AvePoint.RA.RACommonUtility.Common.CommonUtilityForSpecialTenant;

    public class MArchiverService : RMServiceBase, IMArchiverService
    {
        private AveLogger logger = AveLogger.GetInstance(typeof(MArchiverService));
        public IArchiverService ArchiverService { get => new ArchiverService(); set{ } }
        public IEndUserRestoreSettingService EndUserSetting { get => new EndUserRestoreSettingService(); set { } }
        public IRemoteNodeService RemoteNodeService { get => new RemoteNodeService(); set { } }
        private IStorageDeviceService StorageDeviceService => PlatformWindsorManager.GetService<IStorageDeviceService>();
        public IArchiverSiteMasterIndexDao ArchiverSiteMasterIndexService => PlatformWindsorManager.GetService<IArchiverSiteMasterIndexDao>();
        private IRestoreSearchService RestoreSearchService => PlatformWindsorManager.GetService<IRestoreSearchService>();
        private IJobQueueService JobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();
        public IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private IRMRestoreSiteMappingDao RMRestoreSiteMappingDao => PlatformWindsorManager.GetService<IRMRestoreSiteMappingDao>();
        //private IJobMonitorService RMJobService => PlatformWindsorManager.GetService<IJobMonitorService>();
        public async Task<SOReturnMessage> RunEndUserRestoreNow(EndUserRestoreJobConfig jobConfig, bool? runInWebRole = null)
        {
            if (jobConfig == null)
            {
                throw new ArgumentNullException(nameof(jobConfig));
            }

            string siteUrl = GetSiteCollectionUrl(jobConfig, out bool useSiteMapping);

            RemoteSiteCollection site = null; ;
            if (!(jobConfig.PermissionCheckType == CheckPermissionType.StubRestoreLink && IsCancelStubCheckPermission() && IsCancelCheckSiteIfExist()))
            {
                logger.Info($"Need to check if the site exist.365tenantid:{jobConfig.O365TenantId}");
                site = GetRemoteSite(jobConfig, siteUrl);
            }

            SOReturnMessage result = ValidatePermission(jobConfig, site, siteUrl, useSiteMapping, new SPTreeNodeDto());
            if (result.MessageType == SOMessageType.Failed)
            {
                return result;
            }
            else if (result.IsReadOnlySite && jobConfig.RestoreType == GCommon.Contract.StorageOptimization.Object.RestoreType.InPlace)
            {
                return new SOReturnMessage() { MessageType = SOMessageType.Successful, ReturnId = string.Empty, ReturnName = "SiteCollection", IsReadOnlySite = result.IsReadOnlySite };
            }
            var appInfoForAOSP = ArchiverService.GetAOSPAppIdForRestore(jobConfig.O365TenantId);
            if (appInfoForAOSP != null && !string.IsNullOrEmpty(appInfoForAOSP?.Item1) && !string.IsNullOrEmpty(appInfoForAOSP?.Item2))
            {
                if (jobConfig.IsExportJob)
                {
                    jobConfig.RestoreType = GCommon.Contract.StorageOptimization.Object.RestoreType.OutPlace;
                }
                else
                {
                    jobConfig.RestoreType = GCommon.Contract.StorageOptimization.Object.RestoreType.AOPSOop;
                }
                jobConfig.AppProfileId = appInfoForAOSP?.Item1;
                jobConfig.SiteAdminUrl = appInfoForAOSP?.Item2;
                logger.Info($"current aosp appid is:{appInfoForAOSP.Item1},adminurl:{appInfoForAOSP.Item2},restore type:{jobConfig.RestoreType}.AppProfileId:{jobConfig.AppProfileId}.SiteAdminUrl:{jobConfig.SiteAdminUrl}.");
            }
            JobType jobType = ResolveJobType(jobConfig.RestoreType);
            string jobid = EnqueueOrRunJob(jobType, jobConfig, runInWebRole);

            string nodetype = ResolveNodeType(site?.TemplateName == "SPSPERS#2" ? NodeType.SkyDriveProSites : result.SiteCollectionType);
            return new SOReturnMessage() { MessageType = SOMessageType.Successful, ReturnId = jobid, ReturnName = nodetype, IsReadOnlySite = result.IsReadOnlySite };
        }

        private SOReturnMessage ValidatePermission(EndUserRestoreJobConfig jobConfig, RemoteSiteCollection site, string siteUrl, bool useSiteMapping, SPTreeNodeDto siteNode)
        {
            SOReturnMessage matchPermission = null;
            switch (jobConfig.PermissionCheckType)
            {
                case CheckPermissionType.StubRestoreLink:
                    {
                        Stopwatch sw = new Stopwatch();
                        sw.Start();
                        if (IsCancelStubCheckPermission())
                        {
                            logger.Info($"cancel check stub permission. scope: {siteUrl}");
                            matchPermission = new SOReturnMessage() { MessageType = SOMessageType.Successful };
                        }
                        else if (!string.IsNullOrEmpty(jobConfig.OopStubUrl))
                        {
                            matchPermission = ArchiverService.CheckPermissionForStubRestoreLink(site, Uri.UnescapeDataString(jobConfig.OopStubUrl), jobConfig.RunJobUser, "None");
                        }
                        else
                        {
                            matchPermission = ArchiverService.CheckPermissionForStubRestoreLink(site, jobConfig.Items[0].FullPath, jobConfig.RunJobUser, jobConfig.StubType, useSiteMapping);
                        }
                        if (siteNode.Type == NodeType.O365GroupSites && matchPermission.SiteCollectionType == NodeType.O365TeamSites)
                        {
                            siteNode.Type = NodeType.O365TeamSites;
                        }
                        sw.Stop();
                        logger.Info($"linkRestoreReport CheckStubPermission cost time:{sw.ElapsedMilliseconds}");
                        break;
                    }
                case CheckPermissionType.SharePointSite:
                    {
                        matchPermission = ArchiverService.CheckPermissionForSharePointSite(site, jobConfig.RunJobUser);
                        if (siteNode.Type == NodeType.O365GroupSites && matchPermission.SiteCollectionType == NodeType.O365TeamSites)
                        {
                            siteNode.Type = NodeType.O365TeamSites;
                        }
                        break;
                    }
                case CheckPermissionType.GroupOrTeams:
                    {
                        matchPermission = ArchiverService.CheckPermissionForGroupOrTeamSite(site, jobConfig.GroupID, jobConfig.RunJobUser);
                        if (siteNode.Type == NodeType.O365GroupSites && matchPermission.SiteCollectionType == NodeType.O365TeamSites)
                        {
                            siteNode.Type = NodeType.O365TeamSites;
                        }
                        break;
                    }
                case CheckPermissionType.None:
                    {
                        matchPermission = new SOReturnMessage() { MessageType = SOMessageType.Successful };
                        break;
                    }
                default:
                    matchPermission = new SOReturnMessage() { MessageType = SOMessageType.Successful };
                    break;
            }

            return matchPermission;
        }

        private JobType ResolveJobType(GCommon.Contract.StorageOptimization.Object.RestoreType restoreType)
        {
            switch (restoreType)
            {
                case GCommon.Contract.StorageOptimization.Object.RestoreType.InPlace:
                    return JobType.ArchiverRestore;
                case GCommon.Contract.StorageOptimization.Object.RestoreType.StubOop:
                    return JobType.StubOopRestore;
                case GCommon.Contract.StorageOptimization.Object.RestoreType.OutPlace:
                    return JobType.ArchiverOutPlaceRestore;
                case GCommon.Contract.StorageOptimization.Object.RestoreType.AOPSOop:
                    return JobType.AOSPRestore;
                case GCommon.Contract.StorageOptimization.Object.RestoreType.ToSPOLocation:
                    return JobType.ArchiverToSpoRestore;
                default:
                    return JobType.ArchiverRestore;
            }
        }

        private string ResolveNodeType(NodeType siteCollectionType)
        {
            string nodetype = "SiteCollection";
            try
            {
                if (siteCollectionType == NodeType.SkyDriveProSites)
                {
                    nodetype = "SkyDrivePro";
                }
                else if (siteCollectionType == NodeType.O365GroupSites)
                {
                    nodetype = "O365GroupSites";
                }
                else if (siteCollectionType == NodeType.SharePointSites)
                {
                    nodetype = "SiteCollection";
                }
                else if (siteCollectionType == NodeType.O365TeamSites)
                {
                    nodetype = "O365TeamSites";
                }
            }
            catch (Exception e)
            {
                logger.Warn("RunEndUserRestoreNow : {0}", e.ToString());
            }

            return nodetype;
        }

        private RemoteSiteCollection GetRemoteSite(EndUserRestoreJobConfig jobConfig, string siteUrl)
        {
            List<RemoteSiteCollection> sites = RemoteNodeService.GetRemoteSiteCollectionByParam(new List<string> { siteUrl });
            if (sites == null || sites.Count == 0)
            {
                sites = new List<RemoteSiteCollection>() { RemoteNodeService.GetRestoreRemoteNodeFromAosAsync(jobConfig.O365TenantId, siteUrl).GetAwaiter().GetResult() };
                if (sites.First() == null)
                {
                    logger.Warn($"Can not find site {siteUrl} in the remote node.");
                    throw new Exception("Can not find site in the remote node.");
                }
            }

            var site = sites.First();
            if (jobConfig.PermissionCheckType == CheckPermissionType.None)
            {
                logger.Info("this is opus download,permission check type is none");
            }

            return site;
        }

        private string EnqueueOrRunJob(JobType jobType, EndUserRestoreJobConfig jobConfig, bool? runInWebRole)
        {
            TenantLocalValue.LogonUserEmail = jobConfig.RunJobUser;
            string jobParam = SerializerHelper.SerializeByDataContractSerializer(jobConfig);
            if (runInWebRole == true)
            {
                var jqDto = new JobQueueDto()
                {
                    JobType = jobType,
                    TenantGroupId = TenantLocalValue.LogonGroupId,
                    JobRunByUser = TenantLocalValue.LogonUserEmail,
                    JobRunType = JobRunBy.Schedule,
                    Parameters = jobParam,
                    JobPriority = JobPriority.Normal
                };
                var jobid = JobQueueService.AddToDBJobQueue(jqDto);
                logger.Info($"enqueue end user restore job to db job queue, message id:{jobid}");
                return jobid;
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            TenantLocalValue.LogonUserEmail = "RM_TS_RunSchedule";
            var realJobId = RestoreSearchService.RealRunEndUserArchiverRestoreJob(JobRunBy.Schedule, jobConfig.RunJobUser, jobParam, jobType);
            stopwatch.Stop();
            logger.Info($"linkRestoreReport enqueue end user restore job cost time:{stopwatch.ElapsedMilliseconds}");
            return realJobId;
        }

        public string BuildDestinationPathFromFullUrl(string siteCollectionUrl, string fullFileUrl, Dictionary<string, string> libPathMapping)
        {
            try
            {
                var uri = new Uri(fullFileUrl);
                return BuildDestinationPath(siteCollectionUrl, uri.LocalPath, libPathMapping);
            }
            catch (Exception ex)
            {
                logger.Error($"Error in BuildDestinationPathFromFullUrl: {ex.Message}");
                return null;
            }
        }


        public string BuildDestinationPath(string siteCollectionUrl, string fileRelativeUrl, Dictionary<string, string> libPathMapping)
        {
            try
            {
                string fileSubPath = fileRelativeUrl.Trim('/');

                foreach (var item in libPathMapping)
                {
                    var libPath = item.Key.Trim('/');
                    var index = fileSubPath.IndexOf(libPath, StringComparison.OrdinalIgnoreCase);
                    if (index >= 0)
                    {
                        fileSubPath = $"{item.Value}{fileSubPath.Substring(index + libPath.Length).Replace('/', '\\')}";
                        break;
                    }
                }

                return $"{siteCollectionUrl.TrimEnd('/')}\\{fileSubPath}";
            }
            catch (Exception ex)
            {
                logger.Error($"Error in BuildDestinationPath: {ex.Message}");
                return "";
            }
        }

        private bool IsCancelStubCheckPermission()
        {
            var key = RMKeyValueDao.GetValueByKey("IsCancelStubCheckPermission");
            bool.TryParse(key?.Value, out bool result);
            return result;
        }
        private bool IsCancelCheckSiteIfExist()
        {
            var key = RMKeyValueDao.GetValueByKey("IsCancelCheckSiteIfExist");
            bool.TryParse(key?.Value, out bool result);
            return result;
        }

        private string GetSiteCollectionUrl(EndUserRestoreJobConfig jobConfig,out bool useSiteMapping)
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(jobConfig.OopStubUrl))
            {
                var siteMappingInfo = RMRestoreSiteMappingDao.GetMappingBySourceSiteUrl(jobConfig.SiteUrl);
                if (siteMappingInfo != null)
                {
                    result = siteMappingInfo.TargetSiteUrl;
                    useSiteMapping = true;
                    logger.Info($"this enduser restore job is not oop restore,target url:{result}");
                }
                else
                {
                    result = jobConfig.SiteUrl;
                    useSiteMapping = false;
                    logger.Info($"this enduser restore job is not oop restore,url:{result}");
                }

            }
            else
            {
                useSiteMapping = false;
                result = HttpUtility.UrlDecode(GenerateSiteCollecitonUrl(jobConfig.OopStubUrl));
                logger.Info($"this enduser restore job is oop restore,site url:{result}");
            }
            return result;
        }
        private string GenerateSiteCollecitonUrl(string OopStubUrl)
        {
            string result = string.Empty;
            var path = OopStubUrl.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (path.Length > 3 && path[0].StartsWith("https"))
            {
                if (path[2].Equals("sites",StringComparison.OrdinalIgnoreCase) || path[2].Equals("personal", StringComparison.OrdinalIgnoreCase) || path[2].Equals("teams", StringComparison.OrdinalIgnoreCase))
                {
                    result = path[0] + "//" + path[1] + "/" + path[2] + "/" + path[3];
                }
                else
                {
                    result = path[0] + "//" + path[1];
                }
            }
            return result;
        }


        private string GenerateJobId(GCommon.Contract.StorageOptimization.Object.RestoreType restoreType)
        {
            DateTime lastGeneratedDate = DateTime.MinValue;
            string jobId = "";
            try
            {
                DateTime now = DateTime.Now;
                while ((now - lastGeneratedDate) < TimeSpan.FromSeconds(1))
                {
                    Thread.Sleep(1000);
                    now = DateTime.Now;
                }
                lastGeneratedDate = now;
                string prefix = string.Empty;
                if (restoreType == GCommon.Contract.StorageOptimization.Object.RestoreType.InPlace || restoreType == GCommon.Contract.StorageOptimization.Object.RestoreType.StubOop)
                {
                    prefix = "RS";
                }
                else
                {
                    prefix = "ORS";
                }
                jobId = prefix + DateTime.Now.ToString("yyyyMMddHHmmss") + GenerateRandomNumber(6);
            }
            catch (Exception ex)
            {
                logger.Warn("Generating job ID failed: " + ex.ToString());
            }
            return jobId;
        }
        private string GenerateRandomNumber(int count)
        {
            Random ran = new Random((int)DateTime.Now.Ticks);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < count; i++)
            {
                /* Fortify Issue Type: Insecure Randomness 
                * Sink Details: this class StartJobWithRetryAsync 
                * Ignore Reason: random用于 从传入列表中随机选一个值，传入列表值不是固定的，所以是安全的 
                */
                sb.Append(ran.Next(0, 9)).ToString();
            }
            return sb.ToString();
        }
        public async Task<ArchiverStubLink> ParseStubStringAsync(string stubString)
        {
            return await ArchiverService.ParseStubStringAsync(stubString);
        }
        public SOReturnMessage CheckPermissionForStubRestoreLink(RemoteSiteCollection site, string fileUrl, string userMail)
        {
            return ArchiverService.CheckPermissionForStubRestoreLink(site, fileUrl, userMail, "");
        }
        public SOReturnMessage CheckPermissionForSharePointSite(RemoteSiteCollection site, string userMail)
        {
            return ArchiverService.CheckPermissionForSharePointSite(site, userMail);
        }

        public SOReturnMessage CheckPermissionForGroupOrTeamSite(RemoteSiteCollection site, string groupId, string userMail)
        {
            return ArchiverService.CheckPermissionForGroupOrTeamSite(site, groupId, userMail);
        }

        public EndUserRestoreSettingUIDto GetEndUserRestoreSetting()
        {
            return EndUserSetting.GetEndUserRestoreSetting();
        }

        public async Task<EndUserRestoreSettingUIDto> GetEndUserRestoreSettingAsync()
        {
            return await EndUserSetting.GetEndUserRestoreSettingAsync();
        }

        public bool IsExportSizeReachLimited()
        {
            return ArchiverService.IsExportSizeReachLimited();
        }
    }
}
