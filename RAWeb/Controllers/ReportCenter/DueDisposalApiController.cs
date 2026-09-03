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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Box;
using AvePoint.RA.Contract.CodeView;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.RMGeneralSetting;
using AvePoint.RA.Service.TermManagement;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.Filters;
using AvePoint.RA.Web.Common.WIF;
using AvePoint.RA.Web.Models.ReportCenter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;

namespace AvePoint.RA.Web.Controllers.ReportCenter
{
    [RMApiAuthorize(RMPermissionMasks.ReportCenterEnduser, RMReportPermissionMasks.ContentDueForActionEnduser, preferred: false)]
    public class DueDisposalApiController : BaseApiController
    {
        private IRMReportService _RMReportService;
        private IRMReportService RMReportService => PlatformWindsorManager.GetService(ref _RMReportService);
        private IJobMonitorService _JobMonitorService;
        private IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService(ref _JobMonitorService);
        private ISPSettingTreeService _SPSettingTreeService;
        private ISPSettingTreeService SPSettingTreeService => PlatformWindsorManager.GetService(ref _SPSettingTreeService);
        private ILocationManagementService _LocationManagementService;
        private ILocationManagementService LocationManagementService => PlatformWindsorManager.GetService(ref _LocationManagementService);
        private IScheduleService _RMScheduleService;
        private IScheduleService RMScheduleService => PlatformWindsorManager.GetService(ref _RMScheduleService);

        [ValidShowReportQueryPagerActionFilter]
        public Task<string> ShowReportQueryPager([FromBody] ShowReportQuery query)
        {
            return RMReportService.GetCommonReportJobDatasAsync(query);
        }

        [HttpPost]
        public async Task<string> GetProfileReport([FromBody] ShowProfilesReportPageInfo pageInfo)
        {
            ShowProfilesReportPageInfo result = await RMReportService.GetProfilesAsync(pageInfo);
            foreach (var profile in result.Profiles)
            {
                profile.Extension1 = null;
                profile.Extension2 = null;
            }

            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        public async Task<string> GetProfileReportByGenerateReportId([FromBody]int profileId)
        {
            ShowProfilesReportPageInfo pageInfo = new ShowProfilesReportPageInfo
            {
                PageIndex = 1,
                PageSize = 15,
                TotalCount = 0,
                Type = JobType.ItemsFilesDueDisposal,
                IsDesc = true,
                Profiles = null,
                SearchValue = null
            };
            int pageIndex = RMReportService.GetPageIndexByProfileId(profileId);
            pageInfo.PageIndex = pageIndex;
            ShowProfilesReportPageInfo result = await RMReportService.GetProfilesAsync(pageInfo);
            return JsonConvert.SerializeObject(result);
        }

        [HttpGet]
        public async Task<string> ShowReportTimeDropDown([FromQuery]int profileId)
        {
            ShowReportCommonModel model = new ShowReportCommonModel();

            bool hasRanJob = true;
            (model.CollectionTimes,hasRanJob) = await JobMonitorService.GetJobByProfileIdAsync(profileId);

            return JsonConvert.SerializeObject(model);
        }

        /// <summary>
        /// start Due Disposal Report Job
        /// </summary>
        /// <param name="id">profile id</param>
        /// <returns>返回JobId</returns>
        [HttpPost]
        [ValidReportProfileParameterActionFilter]
        public string GenerateReport([FromBody] RMProfileDto profile)
        {
            if (JobTypeConstants.ArchivedSiteReportJobTypes.Contains((int)profile.Type))
            {
                return RMReportService.StartArchivedSiteReportJob(profile.Id);
            }

            return RMReportService.StartReportJob(profile.Type, profile.Id);
        }

        [HttpPost]
        [ValidCreateReportProfileParameterActionFilter]
        public async Task<RAReturnMessage> CreateProfile([FromBody]RMProfileDto profile)
        {
            try
            {
                profile.Extension3 = string.IsNullOrEmpty(profile.Extension3)? profile.Extension3 : SPTreeUtil.BuildSPTreeXMLStr(profile.Extension3);
                if (profile.Extension2.IsNullOrEmpty() || profile.ProfileName.IsNullOrEmpty())
                {
                    throw new ArgumentNullException(nameof(profile));               }

                if (profile.Type == JobType.EXOItemsFilesDueDisposalReport)
                {
                    var EXORoot = SPSettingTreeService.LoadExchangeRoot()[0];
                    profile.Extension2 = string.IsNullOrEmpty(profile.Extension2) ? profile.Extension2 : SPTreeUtil.BuildEXOTreeXMLStr(profile.Extension2, EXORoot.Id);
                }
                else if (profile.Type == JobType.ItemsFilesDueDisposal)
                {
                    var spFarm = SPSettingTreeService.LoadFarm()[0];
                    //profile.Extension2 = string.IsNullOrEmpty(profile.Extension2) ? profile.Extension2 : SPTreeUtil.BuildSPTreeXMLStr(profile.Extension2, spFarm.FarmId);
                    profile.Extension2 = string.IsNullOrEmpty(profile.Extension2) ? profile.Extension2 : SerializerHelper.SerializeByDataContractSerializer(SerializerHelper.DeserializeByJsonConvert<RMSPTreeNode>(profile.Extension2));
                }
                else if (profile.Type == JobType.PhysicalItemsFilesDueDisposalReport)
                {
                    //don't need convert
                    if (!LocationManagementService.CheckPhysicalRootLocation(profile.Extension2))
                    {
                        throw new Exception("Invalid Physical Root Location.");
                    }
                }
                else if (profile.Type == JobType.FSItemsFilesDueDisposal)
                {

                    if (!RMReportService.CheckFSRootNode(profile.Extension2))
                    {
                        throw new Exception("Invalid FS Nodes.");
                    }
                    profile.Extension2 = string.IsNullOrEmpty(profile.Extension2) ? profile.Extension2 : RuleSPTreeUtil.ConvertFSTreeJsonStrToListStr(profile.Extension2);
                }
                else if (profile.Type == JobType.OneDriveItemsFilesDueDisposalReport)
                {
                    //var spFarm = mSPSettingTreeService.LoadFarm()[0];
                    profile.Extension2 = string.IsNullOrEmpty(profile.Extension2) ? profile.Extension2 : SerializerHelper.SerializeByDataContractSerializer(SerializerHelper.DeserializeByJsonConvert<RMSPTreeNode>(profile.Extension2));
                }
                else if (profile.Type == JobType.SPOnPremItemsFilesDueDisposal)
                {
                    //var spFarm = mSPSettingTreeService.LoadFarm()[0];
                    profile.Extension2 = string.IsNullOrEmpty(profile.Extension2) ? profile.Extension2 : SPTreeUtil.BuildSPTreeXMLStr(profile.Extension2, "");
                }
                else if (profile.Type == JobType.BoxItemsFilesDueDisposalReport)
                {
                    profile.Extension2 = string.IsNullOrEmpty(profile.Extension2) ? profile.Extension2 : RuleSPTreeUtil.ConvertBoxTreeJsonStrToListStr(profile.Extension2);
                }
                else if (profile.Type == JobType.GoogleItemsFilesDueDisposalReport)
                {
                    profile.Extension2 = string.IsNullOrEmpty(profile.Extension2) ? profile.Extension2 : RuleSPTreeUtil.ConvertGoogleTreeJsonStrToListStr(profile.Extension2);
                }
                else if (profile.Type == JobType.TeamsItemsFilesDueDisposalReport)
                {
                    profile.Extension2 = string.IsNullOrEmpty(profile.Extension2) ? profile.Extension2 : SerializerHelper.SerializeByDataContractSerializer(SerializerHelper.DeserializeByJsonConvert<RMSPTreeNode>(profile.Extension2));
                }
                else
                {
                    Logger.Error("profile type error: {0}", profile.Type);
                    return new()
                    {
                        MessageType = RAMessageType.Failed,
                        ErrorMessage = "Parameter exception"
                    };
                }
            }
            catch (System.Exception)
            {
                Logger.Error("Build Tree XML Error: {0}", profile.Extension2);
                return new()
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = "Parameter exception"
                };
            }
            try
            {
                RAReturnMessage returnMessage = await RMReportService.BuildProfileAsync(profile);
                if (returnMessage.MessageType == RAMessageType.Failed)
                {
                    Logger.Error("an error occurred while create profile,name:{1},type:{2},ERROR:{0}", returnMessage.ErrorMessage, profile.ProfileName, profile.Type);
                    return returnMessage;
                }
                await UpdateReportScheduleAsync(returnMessage.Extsion1 as RMProfileDto, false);
                Logger.Info("create profile success,name:{0},Type:{1}", profile.ProfileName, profile.Type);
                return returnMessage;
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to create due disposal profile or schedule, Error:{0}", ex);
                return new()
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = "Failed to create profile schedule."
                };
            }
        }

        [HttpPost]
        [ValidReportIdParameterActionFilter]
        public async Task<RMProfileDto> LoadProfileById([FromBody] string Id)
        {
            var profileDto = await RMReportService.GetProfileByIdAsync(Id);
            if (!string.IsNullOrWhiteSpace(profileDto.ScheduleId))
            {
                try
                {
                    profileDto.scheduleInfo = await RMScheduleService.GetScheduleByIdAsync(profileDto.ScheduleId);
                }
                catch
                {
                    profileDto.scheduleInfo = null;
                }
            }
            if (!string.IsNullOrWhiteSpace(profileDto.Extension3))
            {
                    profileDto.Extension3 = SPTreeUtil.ConvertXmlStrToSPTreeJsonStr(profileDto.Extension3);
            }
            if (profileDto.Type == JobType.ItemsFilesDueDisposal)
            {
                ValidReportUtil util = new ValidReportUtil();
                profileDto.Extension2 = string.IsNullOrEmpty(profileDto.Extension2) ? profileDto.Extension2 : await util.GetFilteredSPTreeNodesAsync(SPTreeUtil.ConvertXmlStrToSPTreeJsonStr(profileDto.Extension2), profileDto.Type);
            }
            if (profileDto.Type == JobType.OneDriveItemsFilesDueDisposalReport)
            {
                ValidReportUtil util = new ValidReportUtil();
                profileDto.Extension2 = string.IsNullOrEmpty(profileDto.Extension2) ? profileDto.Extension2 : await util.GetFilteredOneDriveTreeNodesAsync(SPTreeUtil.ConvertXmlStrToSPTreeJsonStr(profileDto.Extension2), profileDto.Type);
            }
            else if (profileDto.Type == JobType.EXOItemsFilesDueDisposalReport)
            {
                ValidReportUtil util = new ValidReportUtil();
                profileDto.Extension2 = string.IsNullOrEmpty(profileDto.Extension2) ? profileDto.Extension2 : await util.GetFilteredEXOTreeNodesAsync(SPTreeUtil.ConvertXmlStrToEXOTreeJsonStr(profileDto.Extension2));
            }
            else if (profileDto.Type == JobType.PhysicalItemsFilesDueDisposalReport)
            {
                //don't need convert
            }
            else if (profileDto.Type == JobType.FSItemsFilesDueDisposal)
            {
                //don't need convert
                profileDto.Extension2 = string.IsNullOrEmpty(profileDto.Extension2) ? profileDto.Extension2 : RuleSPTreeUtil.BuildFSTreeJsonStr(profileDto.Extension2);
            }
            if (profileDto.Type == JobType.SPOnPremItemsFilesDueDisposal)
            {
                ValidReportUtil util = new ValidReportUtil();
                profileDto.Extension2 = string.IsNullOrEmpty(profileDto.Extension2) ? profileDto.Extension2 : SPTreeUtil.ConvertXmlStrToSPTreeJsonStr(profileDto.Extension2);
            }
            if (profileDto.Type == JobType.BoxItemsFilesDueDisposalReport)
            {
                profileDto.Extension2 = string.IsNullOrEmpty(profileDto.Extension2) ? profileDto.Extension2 : RuleSPTreeUtil.BuildBoxTreeJsonStr(profileDto.Extension2);
            }
            if (profileDto.Type == JobType.GoogleItemsFilesDueDisposalReport)
            {
                profileDto.Extension2 = string.IsNullOrEmpty(profileDto.Extension2) ? profileDto.Extension2 : RuleSPTreeUtil.BuildGoogleTreeJsonStr(profileDto.Extension2);
            }
            if (profileDto.Type == JobType.TeamsItemsFilesDueDisposalReport)
            {
                ValidReportUtil util = new ValidReportUtil();
                profileDto.Extension2 = string.IsNullOrEmpty(profileDto.Extension2) ? profileDto.Extension2 : await util.GetFilteredTeamsTreeNodesAsync(SPTreeUtil.ConvertXmlStrToSPTreeJsonStr(profileDto.Extension2), profileDto.Type);
            }
            return profileDto;
        }

        [HttpPost]
        [ValidDeleteReportProfileParameterActionFilter]
        public async Task<List<string>> DeleteProfiles([FromBody]DelProfileInfo dpi)
        {
            Dictionary<int, string> deleteJobProfileNames = new Dictionary<int, string>();
            List<string> CanNotdeleteJobProfileNames = new List<string>();

            for (var i = 0; i < dpi.Ids.Count; i++)
            {
                deleteJobProfileNames.Add(dpi.Ids[i], dpi.Names[i]);
            }
            dpi.ProfileNames = deleteJobProfileNames;
            (_, CanNotdeleteJobProfileNames) = await RMReportService.DeleteProfilesAsync(dpi);
            return CanNotdeleteJobProfileNames;
        }

        [HttpPost]
        [ValidEditReportProfileParameterActionFilter]
        public async Task<RAReturnMessage> EditProfile([FromBody]RMProfileDto profile)
        {
            try
            {
                profile.Extension3 = string.IsNullOrWhiteSpace(profile.Extension3) ? null : SPTreeUtil.BuildSPTreeXMLStr(profile.Extension3);

                if (profile.Extension2.IsNullOrEmpty() || profile.ProfileName.IsNullOrEmpty())
                {
                    throw new ArgumentNullException(nameof(profile));
                }
                if (profile.Type == JobType.EXOItemsFilesDueDisposalReport)
                {
                    var EXORoot = SPSettingTreeService.LoadExchangeRoot()[0];
                    profile.Extension2 = string.IsNullOrEmpty(profile.Extension2) ? profile.Extension2 : SPTreeUtil.BuildEXOTreeXMLStr(profile.Extension2, EXORoot.Id);
                }
                else if (profile.Type == JobType.ItemsFilesDueDisposal)
                {
                    var spFarm = SPSettingTreeService.LoadFarm()[0];
                    profile.Extension2 = string.IsNullOrEmpty(profile.Extension2) ? profile.Extension2 : SerializerHelper.SerializeByDataContractSerializer(SerializerHelper.DeserializeByJsonConvert<RMSPTreeNode>(profile.Extension2));
                }
                else if (profile.Type == JobType.PhysicalItemsFilesDueDisposalReport)
                {
                    //don't need convert
                    if (!LocationManagementService.CheckPhysicalRootLocation(profile.Extension2))
                    {
                        throw new Exception("Invalid Physical Root Location.");
                    }
                }
                else if (profile.Type == JobType.FSItemsFilesDueDisposal)
                {
                    if (!RMReportService.CheckFSRootNode(profile.Extension2))
                    {
                        throw new Exception("Invalid FS Nodes.");
                    }
                    profile.Extension2 = string.IsNullOrEmpty(profile.Extension2) ? profile.Extension2 : RuleSPTreeUtil.ConvertFSTreeJsonStrToListStr(profile.Extension2);
                }
                else if (profile.Type == JobType.OneDriveItemsFilesDueDisposalReport)
                {
                    //var spFarm = mSPSettingTreeService.LoadFarm()[0];
                    profile.Extension2 = string.IsNullOrEmpty(profile.Extension2) ? profile.Extension2 : SerializerHelper.SerializeByDataContractSerializer(SerializerHelper.DeserializeByJsonConvert<RMSPTreeNode>(profile.Extension2));
                }
                else if (profile.Type == JobType.SPOnPremItemsFilesDueDisposal)
                {
                    profile.Extension2 = string.IsNullOrEmpty(profile.Extension2) ? profile.Extension2 : SPTreeUtil.BuildSPTreeXMLStr(profile.Extension2, "");
                }
                else if (profile.Type == JobType.BoxItemsFilesDueDisposalReport)
                {
                    profile.Extension2 = string.IsNullOrEmpty(profile.Extension2) ? profile.Extension2 : RuleSPTreeUtil.ConvertBoxTreeJsonStrToListStr(profile.Extension2);
                }
                else if (profile.Type == JobType.GoogleItemsFilesDueDisposalReport)
                {
                    profile.Extension2 = string.IsNullOrEmpty(profile.Extension2) ? profile.Extension2 : RuleSPTreeUtil.ConvertGoogleTreeJsonStrToListStr(profile.Extension2);
                }
                else if (profile.Type == JobType.TeamsItemsFilesDueDisposalReport)
                {
                    profile.Extension2 = string.IsNullOrEmpty(profile.Extension2) ? profile.Extension2 : SerializerHelper.SerializeByDataContractSerializer(SerializerHelper.DeserializeByJsonConvert<RMSPTreeNode>(profile.Extension2));
                }
                else
                {
                    Logger.Error("profile type error: {0}", profile.Type);
                    return new()
                    {
                        MessageType = RAMessageType.Failed,
                        ErrorMessage = "Parameter exception",
                    };
                }
            }
            catch (System.Exception e)
            {
                Logger.Info("Build Tree XML:{0}, Error: {1}", profile.Extension2, e.ToString());
                return new()
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = "Parameter exception",
                };
            }
            try
            {
                RAReturnMessage returnMessage = await RMReportService.EidtProfileAsync(profile);
                if (returnMessage.MessageType == RAMessageType.Failed)
                {
                    Logger.Error("an error occurred while create profile,name:{1},type:{2},ERROR:{0}", returnMessage.ErrorMessage, profile.ProfileName, profile.Type);
                    return returnMessage;
                }
                await UpdateReportScheduleAsync(profile, true);
                Logger.Info("edit profile success,name:{0},Type:{1}", profile.ProfileName, profile.Type);
                return returnMessage;
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to edit due disposal profile or schedule, Error:{0}", ex);
                return new()
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = "Failed to update profile schedule."
                };
            }
        }

        private async Task UpdateReportScheduleAsync(RMProfileDto profile, bool isEdit)
        {
            if (profile == null)
            {
                return;
            }

            if (profile.scheduleInfo == null || profile.scheduleInfo.NoSchedule)
            {
                if (isEdit && !string.IsNullOrWhiteSpace(profile.ScheduleId))
                {
                    RMScheduleService.DeleteScheduleService(profile.ScheduleId);
                    await RMReportService.UpdateProfileScheduleIdAsync(profile.Id, null);
                }

                profile.ScheduleId = null;
                return;
            }

            profile.scheduleInfo.JobCategory = ScheduleType.ContentDueForAction;
            profile.scheduleInfo.ProfileId = profile.Id.ToString();

            string scheduleId;
            if (string.IsNullOrWhiteSpace(profile.scheduleInfo.Id) || profile.scheduleInfo.Id == "1")
            {
                profile.scheduleInfo.Id = Guid.NewGuid().ToString();
            }
            if (isEdit)
            {
                if (string.IsNullOrWhiteSpace(profile.scheduleInfo.Id))
                {
                    profile.scheduleInfo.Id = profile.ScheduleId;
                }

                scheduleId = await RMScheduleService.UpdateScheduleServiceAsync(profile.scheduleInfo);
            }
            else
            {
                scheduleId = await RMScheduleService.CreateScheduleServiceAsync(profile.scheduleInfo);
            }

            if (string.IsNullOrWhiteSpace(scheduleId) || scheduleId == "-1")
            {
                throw new System.InvalidOperationException("Failed to create or update report schedule.");
            }

            profile.ScheduleId = scheduleId;
            profile.scheduleInfo.Id = scheduleId;
            await RMReportService.UpdateProfileScheduleIdAsync(profile.Id, profile.ScheduleId);
        }
        [RACodeReview("Allen Yin", comment: "没有删临时文件")]
        [HttpPost]
        //[Microsoft.AspNetCore.Mvc.TypeFilter(typeof(ValidateAntiForgeryTokenFilterAttribute))]
        //[FileDownloadFilter]
        public async Task<IActionResult> DownloadFile()
        {
            string jobId = "";
            string profileName = "";
            jobId = HttpUtility.UrlDecode(Request.Form["jobId"]);
            profileName = HttpUtility.UrlDecode(Request.Form["profileName"]);

            HttpResponseMessage response = new HttpResponseMessage();
            BaseJobDto baseJobDto = new BaseJobDto() { Id = jobId, JobType = (int)JobType.ItemsFilesDueDisposal, ProfileName = profileName };
            await RMReportService.GenerateReportAsync(baseJobDto);
            string folderPath = JobReportUtility.GetDownloadReportDetailTempleFolder(baseJobDto);          
            var memoryStream = new MemoryStream();
            using (var stream = new FileStream(folderPath + ".zip", FileMode.Open, FileAccess.Read))
            {
                stream.CopyTo(memoryStream);
            }
            memoryStream.Position = 0;
            return GetValidatedFile(memoryStream, GetContentType(folderPath + ".zip"), Path.GetFileName(folderPath + ".zip"));
        }

        private string GetContentType(string path)
        {
            var provider = new FileExtensionContentTypeProvider();
            string contentType;

            if (!provider.TryGetContentType(path, out contentType))
            {
                contentType = "application/octet-stream";
            }
            return contentType;
        }



    }
}
