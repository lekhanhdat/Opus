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
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.ReportCenter;
using AvePoint.RA.Contract.RMReport;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.Filters;
using AvePoint.RA.Web.Common.WIF;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Newtonsoft.Json;

namespace AvePoint.RA.Web.Controllers.ReportCenter
{
    [RMApiAuthorize(RMPermissionMasks.ReportCenterEnduser, RMReportPermissionMasks.CreationAndDestructionEnduser, preferred: false)]
    public class TimeFrameProfileApiController : BaseApiController
    {
        private IRMReportService _RMReportService;
        private IRMReportService RMReportService => PlatformWindsorManager.GetService(ref _RMReportService);
        private ISPSettingTreeService _SPSettingTreeService;
        private ISPSettingTreeService SPSettingTreeService => PlatformWindsorManager.GetService(ref _SPSettingTreeService);

        private ILocationManagementService _LocationManagementService;
        private ILocationManagementService LocationManagementService => PlatformWindsorManager.GetService(ref _LocationManagementService);

        private ICreateAndDestryoedReportService _CreateAndDestryoedReportService;
        private ICreateAndDestryoedReportService CreateAndDestryoedReportService => PlatformWindsorManager.GetService(ref _CreateAndDestryoedReportService);

        [HttpPost]
        [ValidCreateReportProfileParameterActionFilter]
        public async Task<string> CreateProfile([FromBody]RMProfileDto profile)
        {
            try
            {
                if (profile.Extension2.IsNullOrEmpty() || profile.ProfileName.IsNullOrEmpty())
                {
                    throw new ArgumentNullException(nameof(profile));
                }
                if (!profile.IsCreated && !profile.IsDestoryed)
                {
                    throw new Exception("Profile must be either created or destroyed.");
                }
                if (profile.Type == JobType.EXOCreateAndDestroyedFileReport)
                {
                    var EXORoot = SPSettingTreeService.LoadExchangeRoot()[0];
                    profile.Extension2 = string.IsNullOrEmpty(profile.Extension2) ? profile.Extension2 : SPTreeUtil.BuildEXOTreeXMLStr(profile.Extension2, EXORoot.Id);
                }
                else if (profile.Type == JobType.CreateAndDestroyedFileReport)
                {
                    var spFarm = SPSettingTreeService.LoadFarm()[0];
                    profile.Extension2 = string.IsNullOrEmpty(profile.Extension2) ? profile.Extension2 : SerializerHelper.SerializeByDataContractSerializer(SerializerHelper.DeserializeByJsonConvert<RMSPTreeNode>(profile.Extension2));
                    //profile.Extension2 = string.IsNullOrEmpty(profile.Extension2) ? profile.Extension2 : SPTreeUtil.BuildSPTreeXMLStr(profile.Extension2);
                }
                else if (profile.Type == JobType.TeamsCreateAndDestroyedFileReport)
                { 
                    profile.Extension2 = string.IsNullOrEmpty(profile.Extension2) ? profile.Extension2 : SerializerHelper.SerializeByDataContractSerializer(SerializerHelper.DeserializeByJsonConvert<RMSPTreeNode>(profile.Extension2));
                }
                else if (profile.Type == JobType.PhysicalCreateAndDestroyedFileReport)
                {
                    //don't need convert
                    if (!LocationManagementService.CheckPhysicalRootLocation(profile.Extension2))
                    {
                        throw new Exception("Invalid Physical Root Location.");
                    }
                }
                else if (profile.Type == JobType.FSCreateAndDestroyedFileReport)
                {
                    if (!RMReportService.CheckFSRootNode(profile.Extension2))
                    {
                        throw new Exception("Invalid FS Nodes.");
                    }
                    profile.Extension2 = string.IsNullOrEmpty(profile.Extension2) ? profile.Extension2 : RuleSPTreeUtil.ConvertFSTreeJsonStrToListStr(profile.Extension2);
                }
                else if (profile.Type == JobType.OneDriveCreateAndDestroyedFileReport)
                {
                    profile.Extension2 = string.IsNullOrEmpty(profile.Extension2) ? profile.Extension2 : SerializerHelper.SerializeByDataContractSerializer(SerializerHelper.DeserializeByJsonConvert<RMSPTreeNode>(profile.Extension2));
                }
                else if (profile.Type == JobType.SPOnPremCreateAndDestroyedFileReport)
                {
                    profile.Extension2 = string.IsNullOrEmpty(profile.Extension2) ? profile.Extension2 : SPTreeUtil.BuildSPTreeXMLStr(profile.Extension2);
                }
                else if(profile.Type == JobType.BoxCreateAndDestroyedFileReport)
                {
                    profile.Extension2 = string.IsNullOrEmpty(profile.Extension2) ? profile.Extension2 : RuleSPTreeUtil.ConvertBoxTreeJsonStrToListStr(profile.Extension2);
                }
                else if(profile.Type == JobType.GoogleCreateAndDestroyedFileReport)
                {
                    profile.Extension2 = string.IsNullOrEmpty(profile.Extension2) ? profile.Extension2 : RuleSPTreeUtil.ConvertGoogleTreeJsonStrToListStr(profile.Extension2);
                }
                else
                {
                    Logger.Error("profile type error: {0}", profile.Type);
                    return "Parameter exception";
                }
            }
            catch (System.Exception)
            {
                Logger.Info("Build Tree XML Error: {0}", profile.Extension2);
                return "Parameter exception";
            }
            RAReturnMessage returnMessage = await RMReportService.BuildProfileAsync(profile);
            if (returnMessage.MessageType == RAMessageType.Failed)
            {
                Logger.Error("an error occurred while create profile,name:{1},type:{2},ERROR:{0}", returnMessage.ErrorMessage, profile.ProfileName, profile.Type);
                return returnMessage.ErrorMessage;
            }
            Logger.Info("create profile success,name:{0},Type:{1}", profile.ProfileName, profile.Type);
            return string.Empty;
        }

        [HttpPost]
        [ValidEditReportProfileParameterActionFilter]
        public async Task<string> EditProfile([FromBody]RMProfileDto profile)
        {
            try
            {
                if (profile.Extension2.IsNullOrEmpty() || profile.ProfileName.IsNullOrEmpty())
                {
                    throw new ArgumentNullException(nameof(profile));
                }
                if (!profile.IsCreated && !profile.IsDestoryed)
                {
                    throw new Exception("Profile must be either created or destroyed.");
                }
                if (profile.Type == JobType.EXOCreateAndDestroyedFileReport)
                {
                    var EXORoot = SPSettingTreeService.LoadExchangeRoot()[0];
                    profile.Extension2 = string.IsNullOrEmpty(profile.Extension2) ? profile.Extension2 : SPTreeUtil.BuildEXOTreeXMLStr(profile.Extension2, EXORoot.Id);
                }
                else if (profile.Type == JobType.CreateAndDestroyedFileReport)
                {
                    var spFarm = SPSettingTreeService.LoadFarm()[0];
                    profile.Extension2 = string.IsNullOrEmpty(profile.Extension2) ? profile.Extension2 : SerializerHelper.SerializeByDataContractSerializer(SerializerHelper.DeserializeByJsonConvert<RMSPTreeNode>(profile.Extension2));
                }
                else if (profile.Type == JobType.TeamsCreateAndDestroyedFileReport)
                {    
                    var spFarm = SPSettingTreeService.LoadFarm()[0];
                    profile.Extension2 = string.IsNullOrEmpty(profile.Extension2) ? profile.Extension2 : SerializerHelper.SerializeByDataContractSerializer(SerializerHelper.DeserializeByJsonConvert<RMSPTreeNode>(profile.Extension2));
                }
                else if (profile.Type == JobType.PhysicalCreateAndDestroyedFileReport)
                {
                    //don't need convert
                    if (!LocationManagementService.CheckPhysicalRootLocation(profile.Extension2))
                    {
                        throw new Exception("Invalid Physical Root Location.");
                    }
                }
                else if (profile.Type == JobType.FSCreateAndDestroyedFileReport)
                {
                    if (!RMReportService.CheckFSRootNode(profile.Extension2))
                    {
                        throw new Exception("Invalid FS Nodes.");
                    }
                    profile.Extension2 = string.IsNullOrEmpty(profile.Extension2) ? profile.Extension2 : RuleSPTreeUtil.ConvertFSTreeJsonStrToListStr(profile.Extension2);
                }
                else if (profile.Type == JobType.OneDriveCreateAndDestroyedFileReport)
                {
                    profile.Extension2 = string.IsNullOrEmpty(profile.Extension2) ? profile.Extension2 : SerializerHelper.SerializeByDataContractSerializer(SerializerHelper.DeserializeByJsonConvert<RMSPTreeNode>(profile.Extension2));
                }
                else if (profile.Type == JobType.SPOnPremCreateAndDestroyedFileReport)
                {
                    profile.Extension2 = string.IsNullOrEmpty(profile.Extension2) ? profile.Extension2 : SPTreeUtil.BuildSPTreeXMLStr(profile.Extension2);
                } else if (profile.Type == JobType.BoxCreateAndDestroyedFileReport)
                {
                    profile.Extension2 = string.IsNullOrEmpty(profile.Extension2) ? profile.Extension2 : RuleSPTreeUtil.ConvertBoxTreeJsonStrToListStr(profile.Extension2);
                }
                else if (profile.Type == JobType.GoogleCreateAndDestroyedFileReport)
                {
                    profile.Extension2 = string.IsNullOrEmpty(profile.Extension2) ? profile.Extension2 : RuleSPTreeUtil.ConvertGoogleTreeJsonStrToListStr(profile.Extension2);
                }
                else
                {
                    Logger.Error("profile type error: {0}", profile.Type);
                    return "Parameter exception";
                }
            }
            catch (System.Exception)
            {
                Logger.Info("Build Tree XML Error: {0}", profile.Extension2);
                return "Parameter exception";
            }
            RAReturnMessage returnMessage = await RMReportService.EidtProfileAsync(profile);
            if (returnMessage.MessageType == RAMessageType.Failed)
            {
                Logger.Error("an error occurred while create profile,name:{1},type:{2},ERROR:{0}", returnMessage.ErrorMessage, profile.ProfileName, profile.Type);
                return returnMessage.ErrorMessage;
            }
            Logger.Info("edit profile success,name:{0},Type:{1}", profile.ProfileName, profile.Type);
            return string.Empty;
        }

        [HttpPost]
        [ValidReportIdParameterActionFilter]
        public async Task<RMProfileDto> LoadProfileById([FromBody] string Id)
        {
            var profileDto = await RMReportService.GetProfileByIdAsync(Id);
            if (profileDto.Type == JobType.EXOCreateAndDestroyedFileReport)
            {
                ValidReportUtil util = new ValidReportUtil();
                profileDto.Extension2 = string.IsNullOrEmpty(profileDto.Extension2) ? profileDto.Extension2 : await util.GetFilteredEXOTreeNodesAsync(SPTreeUtil.ConvertXmlStrToEXOTreeJsonStr(profileDto.Extension2));
            }
            else if (profileDto.Type == JobType.PhysicalCreateAndDestroyedFileReport)
            {
                //don't need convert
            }
            else if (profileDto.Type == JobType.FSCreateAndDestroyedFileReport)
            {
                profileDto.Extension2 = string.IsNullOrEmpty(profileDto.Extension2) ? profileDto.Extension2 : RuleSPTreeUtil.BuildFSTreeJsonStr(profileDto.Extension2);
            }
            else if (profileDto.Type == JobType.OneDriveCreateAndDestroyedFileReport)
            {
                ValidReportUtil util = new ValidReportUtil();
                profileDto.Extension2 = string.IsNullOrEmpty(profileDto.Extension2) ? profileDto.Extension2 : await util.GetFilteredOneDriveTreeNodesAsync(SPTreeUtil.ConvertXmlStrToSPTreeJsonStr(profileDto.Extension2), profileDto.Type);
            }
            else if (profileDto.Type == JobType.SPOnPremCreateAndDestroyedFileReport)
            {
                ValidReportUtil util = new ValidReportUtil();
                profileDto.Extension2 = string.IsNullOrEmpty(profileDto.Extension2) ? profileDto.Extension2 : SPTreeUtil.ConvertXmlStrToSPTreeJsonStr(profileDto.Extension2);
            }
            else if (profileDto.Type == JobType.BoxCreateAndDestroyedFileReport)
            {
                profileDto.Extension2 = string.IsNullOrEmpty(profileDto.Extension2) ? profileDto.Extension2 : RuleSPTreeUtil.BuildBoxTreeJsonStr(profileDto.Extension2);
            }
            else if (profileDto.Type == JobType.GoogleCreateAndDestroyedFileReport)
            {
                profileDto.Extension2 = string.IsNullOrEmpty(profileDto.Extension2) ? profileDto.Extension2 : RuleSPTreeUtil.BuildGoogleTreeJsonStr(profileDto.Extension2);
            }
            else if (profileDto.Type == JobType.TeamsCreateAndDestroyedFileReport)
            {
                ValidReportUtil util = new ValidReportUtil();
                profileDto.Extension2 = string.IsNullOrEmpty(profileDto.Extension2) ? profileDto.Extension2 : await util.GetFilteredTeamsTreeNodesAsync(SPTreeUtil.ConvertXmlStrToSPTreeJsonStr(profileDto.Extension2), profileDto.Type);
            }
            else
            {
                ValidReportUtil util = new ValidReportUtil();
                profileDto.Extension2 = string.IsNullOrEmpty(profileDto.Extension2) ? profileDto.Extension2 : await util.GetFilteredSPTreeNodesAsync(SPTreeUtil.ConvertXmlStrToSPTreeJsonStr(profileDto.Extension2), profileDto.Type);
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
                Type = JobType.CreateAndDestroyedFileReport,
                IsDesc = true,
                Profiles = null,
                SearchValue = null
            };
            int pageIndex = RMReportService.GetPageIndexByProfileId(profileId);
            pageInfo.PageIndex = pageIndex;
            ShowProfilesReportPageInfo result = await RMReportService.GetProfilesAsync(pageInfo);
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        [ValidReportProfileParameterActionFilter]
        public string GenerateReport([FromBody] RMProfileDto profile)
        {
            return RMReportService.StartReportJob(profile.Type, profile.Id);
        }

        [ValidShowReportQueryPagerActionFilter]
        public Task<string> ShowReportQueryPager([FromBody] ShowReportQuery query)
        {
            return RMReportService.GetCommonReportJobDatasAsync(query);
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin)]
        public bool GenerateSiteMetricsReportJob()
        {
            return CreateAndDestryoedReportService.RunSiteMetricsReportJob();
        }

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
            BaseJobDto baseJobDto = new BaseJobDto() { Id = jobId, JobType = (int)JobType.CreateAndDestroyedFileReport, ProfileName = profileName };
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