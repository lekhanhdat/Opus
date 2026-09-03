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
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMReport;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.RACommonUtility.Permission;
using AvePoint.RA.Service.Services.AccountManager;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.Filters;
using AvePoint.RA.Web.Common.WIF;
using AvePoint.RA.Web.Models.ReportCenter;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Common.Global.Utils;
using System.Threading.Tasks;
using ExportReportCommonModel = AvePoint.RA.Contract.RMReport.ExportReportCommonModel;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.Contract.RMWeb.ReportCenter;

namespace AvePoint.RA.Web.Controllers.ReportCenter
{
    //CommonModuleAccess 临时对标 ReportCenterEnduser
    [RMApiAuthorize(RMPermissionMasks.ReportCenterEnduser | RMPermissionMasks.PhysicalEndUser, RMSOPermissionMasks.CommonModuleAccess, RMReportPermissionMasks.RuleUsageEnduser | RMReportPermissionMasks.ActionAuditEnduser | RMReportPermissionMasks.RestoredDataEnduser | RMReportPermissionMasks.TermUsageEnduser | RMReportPermissionMasks.CreationAndDestructionEnduser | RMReportPermissionMasks.ContentDueForActionEnduser, preferred: false)]
    public class RCApiController : BaseApiController
    {
        private IRMReportService _RMReportService;
        private IRMReportService RMReportService => PlatformWindsorManager.GetService(ref _RMReportService);
        private IJobMonitorService _JobMonitorService;
        private IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService(ref _JobMonitorService);
        private IRMSecurityTrimmingHelper _SecurityTrimmingHelper;
        private IRMSecurityTrimmingHelper SecurityTrimmingHelper => PlatformWindsorManager.GetService(ref _SecurityTrimmingHelper);

        private IRMKeyValueDao _RMKeyValueDao;
        private IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService(ref _RMKeyValueDao);

        [HttpGet]
        public async Task<ShowReportCommonModel> CommonShowReport(JobType reportType, string profileId = null, string jobId = null)
        {
            try
            {
                ShowReportCommonModel model = new ShowReportCommonModel();
                model.ProfileNames = new List<ProfileSimpleInfo>();

                var isSPAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.SPOEnduser);
                var isEXOAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.EXOEnduser);
                var isPhyAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.PhysicalAdmin);
                var isFSAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.FSAdmin);
                var isOneDriveAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.OneDriveEnduser);
                var isSPOnPremAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.SPOnPremEnduser);
                var isBoxAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.BoxAdmin);
                var isGoogleAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.GoogleAdmin);
                var isTeamsAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.TeamsEndUser);

                var isSOSPAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMSOPermissionMasks.SPOEnduser);
                var isSOOneDriveAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMSOPermissionMasks.OneDriveEnduser);
                var isSOTeamsAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMSOPermissionMasks.TeamsEndUser);

                var isEnableJPMCFeature = RMKeyValueDao.IsEnableJPMCFileSystemFeature();

                var associatedReportTypes = new List<JobType>();
                var sources = new List<SourceFlag>()
                {
                    SourceFlag.All,
                };
                switch (reportType)
                {
                    case JobType.DisposalReport:
                    case JobType.ItemsFilesDueDisposal:
                    case JobType.EXOItemsFilesDueDisposalReport:
                    case JobType.PhysicalItemsFilesDueDisposalReport:
                    case JobType.FSItemsFilesDueDisposal:
                    case JobType.OneDriveItemsFilesDueDisposalReport:
                    case JobType.SPOnPremItemsFilesDueDisposal:
                    case JobType.BoxItemsFilesDueDisposalReport:
                    case JobType.GoogleItemsFilesDueDisposalReport:
                    case JobType.TeamsItemsFilesDueDisposalReport:
                        if (isSPAdmin)
                        {
                            sources.Add(SourceFlag.SharePoint);
                            associatedReportTypes.Add(JobType.ItemsFilesDueDisposal);
                        }
                        if (isEXOAdmin)
                        {
                            sources.Add(SourceFlag.Exchange);
                            associatedReportTypes.Add(JobType.EXOItemsFilesDueDisposalReport);
                        }
                        if (isPhyAdmin)
                        {
                            sources.Add(SourceFlag.Physical);
                            associatedReportTypes.Add(JobType.PhysicalItemsFilesDueDisposalReport);
                        }
                        if (isFSAdmin && !isEnableJPMCFeature)
                        {
                            sources.Add(SourceFlag.FileSystem);
                            associatedReportTypes.Add(JobType.FSItemsFilesDueDisposal);
                        }
                        if (isOneDriveAdmin)
                        {
                            sources.Add(SourceFlag.OneDrive);
                            associatedReportTypes.Add(JobType.OneDriveItemsFilesDueDisposalReport);
                        }
                        if (isSPOnPremAdmin)
                        {
                            sources.Add(SourceFlag.SharePointOnPrem);
                            associatedReportTypes.Add(JobType.SPOnPremItemsFilesDueDisposal);
                        }
                        if (isBoxAdmin)
                        {
                            sources.Add(SourceFlag.Box);
                            associatedReportTypes.Add(JobType.BoxItemsFilesDueDisposalReport);
                        }
                        if (isGoogleAdmin)
                        {
                            sources.Add(SourceFlag.Google);
                            associatedReportTypes.Add(JobType.GoogleItemsFilesDueDisposalReport);
                        }
                        if(isTeamsAdmin)
                        {
                            sources.Add(SourceFlag.Teams);
                            associatedReportTypes.Add(JobType.TeamsItemsFilesDueDisposalReport);
                        }
                        associatedReportTypes.Add(JobType.DisposalReport);
                        break;
                    case JobType.ArchivedSiteReport:
                    case JobType.OneDriveArchivedSiteReport:
                    case JobType.TeamsArchivedSiteReport:
                    case JobType.GoogleArchivedSiteReport:
                        associatedReportTypes.Add(reportType);
                        break;
                    case JobType.BCSTermUsageReport:
                    case JobType.EXOTermUsageReport:
                    case JobType.PhysicalTermUsageReport:
                    case JobType.FSBCSTermUsageReport:
                    case JobType.OneDriveTermUsageReport:
                    case JobType.RetiredTermReport:
                    case JobType.EXORetiredTermUsageReport:
                    case JobType.PhysicalRetiredTermUsageReport:
                    case JobType.FSRetiredTermReport:
                    case JobType.OneDriveRetiredTermUsageReport:
                    case JobType.OrphanedTermReport:
                    case JobType.EXOOrphanedTermUsageReport:
                    case JobType.PhysicalOrphanedTermUsageReport:
                    case JobType.FSOrphanedTermReport:
                    case JobType.OneDriveOrphanedTermUsageReport:
                    case JobType.SPOnPremBCSTermUsageReport:
                    case JobType.SPOnPremRetiredTermReport:
                    case JobType.SPOnPremOrphanedTermReport:
                    case JobType.BoxBCSTermUsageReport:
                    case JobType.BoxOrphanedTermUsageReport:
                    case JobType.BoxRetiredTermUsageReport:
                    case JobType.GoogleBCSTermUsageReport:
                    case JobType.GoogleOrphanedTermUsageReport:
                    case JobType.GoogleRetiredTermUsageReport:
                    case JobType.TeamsBCSTermUsageReport:
                    case JobType.TeamsOrphanedTermUsageReport:
                    case JobType.TeamsRetiredTermUsageReport:
                        if (isSPAdmin)
                        {
                            associatedReportTypes.Add(JobType.BCSTermUsageReport);
                            associatedReportTypes.Add(JobType.RetiredTermReport);
                            associatedReportTypes.Add(JobType.OrphanedTermReport);
                        }
                        if (isEXOAdmin)
                        {
                            associatedReportTypes.Add(JobType.EXOTermUsageReport);
                            associatedReportTypes.Add(JobType.EXORetiredTermUsageReport);
                            associatedReportTypes.Add(JobType.EXOOrphanedTermUsageReport);
                        }
                        if (isPhyAdmin)
                        {
                            associatedReportTypes.Add(JobType.PhysicalTermUsageReport);
                            associatedReportTypes.Add(JobType.PhysicalRetiredTermUsageReport);
                            associatedReportTypes.Add(JobType.PhysicalOrphanedTermUsageReport);
                        }
                        if (isFSAdmin && !isEnableJPMCFeature)
                        {
                            associatedReportTypes.Add(JobType.FSBCSTermUsageReport);
                            associatedReportTypes.Add(JobType.FSRetiredTermReport);
                            associatedReportTypes.Add(JobType.FSOrphanedTermReport);
                        }
                        if (isOneDriveAdmin)
                        {
                            associatedReportTypes.Add(JobType.OneDriveTermUsageReport);
                            associatedReportTypes.Add(JobType.OneDriveRetiredTermUsageReport);
                            associatedReportTypes.Add(JobType.OneDriveOrphanedTermUsageReport);
                        }
                        if (isSPOnPremAdmin)
                        {
                            associatedReportTypes.Add(JobType.SPOnPremBCSTermUsageReport);
                            associatedReportTypes.Add(JobType.SPOnPremRetiredTermReport);
                            associatedReportTypes.Add(JobType.SPOnPremOrphanedTermReport);
                        }
                        if (isBoxAdmin)
                        {
                            associatedReportTypes.Add(JobType.BoxBCSTermUsageReport);
                            associatedReportTypes.Add(JobType.BoxOrphanedTermUsageReport);
                            associatedReportTypes.Add(JobType.BoxRetiredTermUsageReport);
                        }
                        if (isGoogleAdmin)
                        {
                            associatedReportTypes.Add(JobType.GoogleBCSTermUsageReport);
                            associatedReportTypes.Add(JobType.GoogleOrphanedTermUsageReport);
                            associatedReportTypes.Add(JobType.GoogleRetiredTermUsageReport);
                        }
                        if (isTeamsAdmin)
                        {
                            associatedReportTypes.Add(JobType.TeamsBCSTermUsageReport);
                            associatedReportTypes.Add(JobType.TeamsOrphanedTermUsageReport);
                            associatedReportTypes.Add(JobType.TeamsRetiredTermUsageReport);
                        }
                        break;
                    case JobType.CreateAndDestroyedFileReport:
                    case JobType.EXOCreateAndDestroyedFileReport:
                    case JobType.PhysicalCreateAndDestroyedFileReport:
                    case JobType.FSCreateAndDestroyedFileReport:
                    case JobType.OneDriveCreateAndDestroyedFileReport:
                    case JobType.SPOnPremCreateAndDestroyedFileReport:
                    case JobType.CreateAndDestroyedReport:
                    case JobType.BoxCreateAndDestroyedFileReport:
                    case JobType.GoogleCreateAndDestroyedFileReport:
                    case JobType.TeamsCreateAndDestroyedFileReport:
                        if (isSPAdmin)
                        {
                            sources.Add(SourceFlag.SharePoint);
                            associatedReportTypes.Add(JobType.CreateAndDestroyedFileReport);
                        }
                        if (isEXOAdmin)
                        {
                            sources.Add(SourceFlag.Exchange);
                            associatedReportTypes.Add(JobType.EXOCreateAndDestroyedFileReport);
                        }
                        if (isPhyAdmin)
                        {
                            sources.Add(SourceFlag.Physical);
                            associatedReportTypes.Add(JobType.PhysicalCreateAndDestroyedFileReport);
                        }
                        if (isFSAdmin && !isEnableJPMCFeature)
                        {
                            sources.Add(SourceFlag.FileSystem);
                            associatedReportTypes.Add(JobType.FSCreateAndDestroyedFileReport);
                        }
                        if (isOneDriveAdmin)
                        {
                            sources.Add(SourceFlag.OneDrive);
                            associatedReportTypes.Add(JobType.OneDriveCreateAndDestroyedFileReport);
                        }
                        if (isSPOnPremAdmin)
                        {
                            sources.Add(SourceFlag.SharePointOnPrem);
                            associatedReportTypes.Add(JobType.SPOnPremCreateAndDestroyedFileReport);
                        }
                        if (isBoxAdmin)
                        {
                            sources.Add(SourceFlag.Box);
                            associatedReportTypes.Add(JobType.BoxCreateAndDestroyedFileReport);
                        }
                        if (isGoogleAdmin)
                        {
                            sources.Add(SourceFlag.Google);
                            associatedReportTypes.Add(JobType.GoogleCreateAndDestroyedFileReport);
                        }
                        if (isTeamsAdmin)
                        {
                            sources.Add(SourceFlag.Teams);
                            associatedReportTypes.Add(JobType.TeamsCreateAndDestroyedFileReport);
                        }
                        associatedReportTypes.Add(JobType.CreateAndDestroyedReport);
                        break;
                    case JobType.SPOActionAuditReport:
                    case JobType.OneDriveActionAuditReport:
                    case JobType.TeamsActionAuditReport:
                        if (isSOSPAdmin || isSPAdmin)
                        {
                            associatedReportTypes.Add(JobType.SPOActionAuditReport);
                        }
                        if (isSOOneDriveAdmin || isOneDriveAdmin)
                        {
                            associatedReportTypes.Add(JobType.OneDriveActionAuditReport);
                        }
                        if (isTeamsAdmin || isSOTeamsAdmin)
                        {
                            associatedReportTypes.Add(JobType.TeamsActionAuditReport);
                        }
                        break;
                    case JobType.RestoreReport:
                    case JobType.OneDriverRestoreReport:
                    case JobType.TeamsRestoreReport:
                    case JobType.GoogleRestoreReport:
                        if (isSPAdmin || isSOSPAdmin)
                        {
                            sources.Add(SourceFlag.SharePoint);
                            associatedReportTypes.Add(JobType.RestoreReport);
                        }
                        if (isOneDriveAdmin || isSOOneDriveAdmin)
                        {
                            sources.Add(SourceFlag.OneDrive);
                            associatedReportTypes.Add(JobType.OneDriverRestoreReport);
                        }
                        if(isTeamsAdmin || isSOTeamsAdmin)
                        {
                            sources.Add(SourceFlag.Teams);
                            associatedReportTypes.Add(JobType.TeamsRestoreReport);
                        }
                        if (isGoogleAdmin)
                        {
                            sources.Add(SourceFlag.Google);
                            associatedReportTypes.Add(JobType.GoogleRestoreReport);
                        }
                        break;
                    default:
                        associatedReportTypes.Add(reportType);
                        break;
                }
                var profiles = await RMReportService.GetProfilesByTypesAsync(associatedReportTypes, sources);
                model.ProfileNames.AddRange(profiles);

                bool hasRanJob = false;
                List<JobType> onlyShowFinishJobTypeList = new List<JobType>
                {
                    JobType.RestoreReport,
                    JobType.OneDriverRestoreReport,
                    JobType.TeamsRestoreReport
                };
                // Keep Archived Sites reports on completed-job readiness flow.
                bool onlyShowFinishJob = onlyShowFinishJobTypeList.Contains(reportType)
                    || JobTypeConstants.ArchivedSiteReportJobTypes.Contains((int)reportType);
                if (model.ProfileNames.Count > 0)
                {
                    //从report页面进入
                    if (!string.IsNullOrEmpty(profileId))
                    {
                        model.ProfileId = profileId;
                        (model.CollectionTimes,hasRanJob) = await JobMonitorService.GetJobByProfileIdAsync(int.Parse(profileId), onlyShowFinishJob);
                    }
                    //从job monitor 进入
                    else if (!string.IsNullOrEmpty(jobId))
                    {
                        var job = await JobMonitorService.GetJobAsync(jobId);
                        if (job != null)
                        {
                            model.ProfileId = job.ProfileId.ToString();
                            (model.CollectionTimes, hasRanJob) = await JobMonitorService.GetJobByProfileIdAsync(job.ProfileId, onlyShowFinishJob);
                        }
                    }
                    //没有传入任何id进入此页面
                    else
                    {
                        model.ProfileId = "";
                        model.JobId = "";
                        (model.CollectionTimes, hasRanJob) = await JobMonitorService.GetJobByProfileIdAsync(int.Parse(model.ProfileNames[0].Id), onlyShowFinishJob);
                    }
                }
                else
                {
                    model.CollectionTimes = new List<KeyValuePair<string, string>>();
                }
                if (!hasRanJob)
                {
                    model.ProfileNames = new List<ProfileSimpleInfo>();//没有run Job不需要返回此信息
                }
                model.HasRanJob = hasRanJob;
                return model;
            }
            catch (Exception)
            {
                Logger.Warn("Invalidate profileId and jobId");
                return new ShowReportCommonModel()
                {
                    JobId = "",
                    ProfileId = "",
                    CollectionTimes = new List<KeyValuePair<string, string>>(),
                    ProfileNames = new List<ProfileSimpleInfo>()
                };
            }
        }

        [HttpPost]
        [ValidShowReportQueryPagerActionFilter]
        public Task<string> ShowReportQueryPager([FromBody] ShowReportQuery query)
        {
            return RMReportService.GetCommonReportJobDatasAsync(query);
        }

        [HttpPost]
        public RAReturnMessage RunCommonExportReportJob([FromBody] ExportReportCommonModel commonModel)
        {
            var parameter = SerializerHelper.SerializeByJsonConvert(commonModel);
            return RMReportService.RunExportReportJob(parameter);
        }
    }
}