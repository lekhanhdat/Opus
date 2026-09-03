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
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.Filters;
using AvePoint.RA.Web.Common.WIF;
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
    [RMApiAuthorize(RMPermissionMasks.ReportCenterEnduser, RMReportPermissionMasks.TermUsageEnduser, preferred: false)]
    public class TermUsageReportApiController : BaseApiController
    {
        private ISPSettingTreeService _SPSettingTreeService;
        private ISPSettingTreeService SPSettingTreeService => PlatformWindsorManager.GetService(ref _SPSettingTreeService);
        private IGlobalSettingService _GlobalSettingService;
        private IGlobalSettingService GlobalSettingService => PlatformWindsorManager.GetService(ref _GlobalSettingService);
        private IRMReportService _RMReportService;
        private IRMReportService RMReportService => PlatformWindsorManager.GetService(ref _RMReportService);
        private ITaxonomyService _TaxonomyService;
        private ITaxonomyService TaxonomyService => PlatformWindsorManager.GetService(ref _TaxonomyService);
        private ILocationManagementService _LocationManagementService;
        private ILocationManagementService LocationManagementService => PlatformWindsorManager.GetService(ref _LocationManagementService);

        [HttpPost]
        public async Task<string> GetTermsUsageReport([FromBody] ShowProfilesReportPageInfo pageInfo)
        {
            ShowProfilesReportPageInfo result = await RMReportService.GetTermUsageAndOrphanedTermProfilesAsync(pageInfo);
            foreach (var profile in result.Profiles)
            {
                profile.Extension1 = null;
                profile.Extension2 = null;
            }

            return JsonConvert.SerializeObject(result);
        }

        //[HttpPost]
        //public string GetProfileReportByGenerateReportId([FromBody]int profileId)
        //{
        //    ShowProfilesReportPageInfo pageInfo = new ShowProfilesReportPageInfo
        //    {
        //        PageIndex = 1,
        //        PageSize = 15,
        //        TotalCount = 0,
        //        Type = JobType.BCSTermUsageReport,
        //        IsDesc = true,
        //        Profiles = null,
        //        SearchValue = null
        //    };
        //    int pageIndex = mRMReportService.GetPageIndexByProfileId(profileId);
        //    pageInfo.PageIndex = pageIndex;
        //    ShowProfilesReportPageInfo result = mRMReportService.GetProfiles(pageInfo);
        //    return JsonConvert.SerializeObject(result);
        //}

        [HttpPost]
        public ValidationMessage ValidateDAConnectionSetting()
        {
            return GlobalSettingService.CheckDocAveConnectionSetting();
        }

        [RACodeReview("Allen Yin")]
        [HttpPost]
        [ValidCreateReportProfileParameterActionFilter]
        public async Task<string> CreateProfile([FromBody]RMProfileDto profile)
        {
            try
            {
                try
                {
                    if (profile.Extension2.IsNullOrEmpty() || profile.ProfileName.IsNullOrEmpty())
                    {
                        throw new ArgumentNullException(nameof(profile));
                    }
                    if ((profile.Type == JobType.FSBCSTermUsageReport || profile.Type == JobType.EXOTermUsageReport
                        || profile.Type == JobType.BCSTermUsageReport || profile.Type == JobType.PhysicalTermUsageReport
                        || profile.Type == JobType.OneDriveTermUsageReport || profile.Type == JobType.SPOnPremBCSTermUsageReport
                        || profile.Type == JobType.BoxBCSTermUsageReport || profile.Type == JobType.GoogleBCSTermUsageReport 
                        || profile.Type == JobType.TeamsBCSTermUsageReport) && profile.Extension1.IsNullOrEmpty())
                    {
                        throw new ArgumentNullException(nameof(profile));
                    }
                    if (profile.Type == JobType.FSBCSTermUsageReport || profile.Type == JobType.FSOrphanedTermReport || profile.Type == JobType.FSRetiredTermReport)
                    {
                        if (!RMReportService.CheckFSRootNode(profile.Extension2))
                        {
                            throw new Exception("Invalid FS Nodes.");
                        }
                        profile.Extension2 = string.IsNullOrEmpty(profile.Extension2) ? profile.Extension2 : SerializerHelper.SerializeByDataContractSerializer(SerializerHelper.DeserializeByJsonConvert<RMFSTreeNode>(profile.Extension2));
                    }
                    else if ((int)profile.Type >= (int)JobType.EXOTermUsageReport && (int)profile.Type <= (int)JobType.EXORetiredTermUsageReport)
                    {
                        var EXORoot = SPSettingTreeService.LoadExchangeRoot()[0];
                        profile.Extension2 = string.IsNullOrEmpty(profile.Extension2) ? profile.Extension2 : SPTreeUtil.BuildEXOTreeXMLStr(profile.Extension2, EXORoot.Id);
                    }
                    else if (profile.Type == JobType.BCSTermUsageReport || profile.Type == JobType.OrphanedTermReport || profile.Type == JobType.RetiredTermReport)
                    {
                        var spFarm = SPSettingTreeService.LoadFarm()[0];
                        profile.Extension2 = string.IsNullOrEmpty(profile.Extension2) ? profile.Extension2 : SerializerHelper.SerializeByDataContractSerializer(SerializerHelper.DeserializeByJsonConvert<RMSPTreeNode>(profile.Extension2));
                        //profile.Extension2 = string.IsNullOrEmpty(profile.Extension2) ? profile.Extension2 : SPTreeUtil.BuildSPTreeXMLStr(profile.Extension2, spFarm.FarmId);
                    }
                    else if (profile.Type == JobType.PhysicalTermUsageReport || profile.Type == JobType.PhysicalOrphanedTermUsageReport || profile.Type == JobType.PhysicalRetiredTermUsageReport)
                    {
                        //don't need convert
                        if (!LocationManagementService.CheckPhysicalRootLocation(profile.Extension2)) {
                            throw new Exception("Invalid Physical Root Location.");
                        }
                    }
                    else if (profile.Type == JobType.OneDriveTermUsageReport || profile.Type == JobType.OneDriveOrphanedTermUsageReport || profile.Type == JobType.OneDriveRetiredTermUsageReport)
                    {
                        //don't need convert
                        //var spFarm = mSPSettingTreeService.LoadFarm()[0];
                        profile.Extension2 = string.IsNullOrEmpty(profile.Extension2) ? profile.Extension2 : SerializerHelper.SerializeByDataContractSerializer(SerializerHelper.DeserializeByJsonConvert<RMSPTreeNode>(profile.Extension2));
                    }
                    else if (profile.Type == JobType.SPOnPremBCSTermUsageReport || profile.Type == JobType.SPOnPremOrphanedTermReport || profile.Type == JobType.SPOnPremRetiredTermReport)
                    {
                        //don't need convert
                        //var spFarm = mSPSettingTreeService.LoadFarm()[0];
                        profile.Extension2 = string.IsNullOrEmpty(profile.Extension2) ? profile.Extension2 : SPTreeUtil.BuildSPTreeXMLStr(profile.Extension2, "");
                    }
                    else if (profile.Type == JobType.BoxBCSTermUsageReport || profile.Type == JobType.BoxOrphanedTermUsageReport || profile.Type == JobType.BoxRetiredTermUsageReport)
                    {
                        if (!RMReportService.CheckBoxRootNode(profile.Extension2))
                        {
                            throw new Exception("Invalid Box Nodes.");
                        }
                        profile.Extension2 = string.IsNullOrEmpty(profile.Extension2) ? profile.Extension2 : SerializerHelper.SerializeByDataContractSerializer(SerializerHelper.DeserializeByJsonConvert<BoxTreeNode>(profile.Extension2));
                    }
                    else if (profile.Type == JobType.GoogleBCSTermUsageReport || profile.Type == JobType.GoogleOrphanedTermUsageReport || profile.Type == JobType.GoogleRetiredTermUsageReport)
                    {
                        profile.Extension2 = string.IsNullOrEmpty(profile.Extension2) ? profile.Extension2 : SerializerHelper.SerializeByDataContractJsonSerializer(SerializerHelper.DeserializeByJsonConvert<RMGoogleTreeNode>(profile.Extension2));
                    }
                    else if (profile.Type == JobType.TeamsBCSTermUsageReport || profile.Type == JobType.TeamsOrphanedTermUsageReport || profile.Type == JobType.TeamsRetiredTermUsageReport)
                    {
                        profile.Extension2 = string.IsNullOrEmpty(profile.Extension2) ? profile.Extension2 : SerializerHelper.SerializeByDataContractSerializer(SerializerHelper.DeserializeByJsonConvert<RMSPTreeNode>(profile.Extension2));
                    }
                    else
                    {
                        Logger.Error("profile type error: {0}", profile.Type);
                        return "Parameter exception";
                    }
                }
                catch (Exception)
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
            }

            catch (Exception e)
            {
                var returnMessage = string.Empty;
                if (e.Message.Contains("The field Description must be a string or array type with a maximum length of '255'."))
                {
                    returnMessage = I18NEntity.GetString("RM_JS_Profile_Description_TooLong");
                }
                Logger.Info("create profile failed,name:{0},Type:{1},Error:{2}", profile.ProfileName, profile.Type, e.ToString());
                return returnMessage;
            }
            return string.Empty;
        }

        [RACodeReview("Allen Yin")]
        [HttpPost]
        [ValidReportIdParameterActionFilter]
        public async Task<RMProfileDto> LoadProfileById([FromBody] string Id)
        {
            var profileDto = await RMReportService.GetProfileByIdAsync(Id);
            ValidReportUtil util = new ValidReportUtil();
            if (profileDto.Type == JobType.FSBCSTermUsageReport || profileDto.Type == JobType.FSOrphanedTermReport || profileDto.Type == JobType.FSRetiredTermReport)
            {
                profileDto.Extension2 = string.IsNullOrEmpty(profileDto.Extension2) ? profileDto.Extension2 : RuleSPTreeUtil.ConvertXmlStrToFSTreeStr(profileDto.Extension2);
            }
            else if ((int)profileDto.Type >= 2101 && (int)profileDto.Type <= 2103)
            {
                profileDto.Extension2 = string.IsNullOrEmpty(profileDto.Extension2) ? profileDto.Extension2 : await util.GetFilteredEXOTreeNodesAsync(SPTreeUtil.ConvertXmlStrToEXOTreeJsonStr(profileDto.Extension2));
            }
            else if (profileDto.Type == JobType.BCSTermUsageReport || profileDto.Type == JobType.OrphanedTermReport || profileDto.Type == JobType.RetiredTermReport)
            {
                profileDto.Extension2 = string.IsNullOrEmpty(profileDto.Extension2) ? profileDto.Extension2 : await util.GetFilteredSPTreeNodesAsync(SPTreeUtil.ConvertXmlStrToSPTreeJsonStr(profileDto.Extension2), profileDto.Type);
            }
            else if (profileDto.Type == JobType.PhysicalTermUsageReport || profileDto.Type == JobType.PhysicalOrphanedTermUsageReport || profileDto.Type == JobType.PhysicalRetiredTermUsageReport)
            {
                //don't need convert
            }
            else if (profileDto.Type == JobType.OneDriveTermUsageReport || profileDto.Type == JobType.OneDriveOrphanedTermUsageReport || profileDto.Type == JobType.OneDriveRetiredTermUsageReport)
            {
                profileDto.Extension2 = string.IsNullOrEmpty(profileDto.Extension2) ? profileDto.Extension2 : await util.GetFilteredOneDriveTreeNodesAsync(SPTreeUtil.ConvertXmlStrToSPTreeJsonStr(profileDto.Extension2), profileDto.Type);
            }
            else if (profileDto.Type == JobType.SPOnPremBCSTermUsageReport || profileDto.Type == JobType.SPOnPremOrphanedTermReport || profileDto.Type == JobType.SPOnPremRetiredTermReport)
            {
                profileDto.Extension2 = string.IsNullOrEmpty(profileDto.Extension2) ? profileDto.Extension2 : SPTreeUtil.ConvertXmlStrToSPTreeJsonStr(profileDto.Extension2);
            }
            else if (profileDto.Type == JobType.BoxBCSTermUsageReport || profileDto.Type == JobType.BoxOrphanedTermUsageReport || profileDto.Type == JobType.BoxRetiredTermUsageReport)
            {
                profileDto.Extension2 = string.IsNullOrEmpty(profileDto.Extension2) ? profileDto.Extension2 : RuleSPTreeUtil.ConvertXmlStrToBoxTreeStr(profileDto.Extension2);
            }
            else if (profileDto.Type == JobType.GoogleBCSTermUsageReport || profileDto.Type == JobType.GoogleOrphanedTermUsageReport || profileDto.Type == JobType.GoogleRetiredTermUsageReport)
            {
                profileDto.Extension2 = string.IsNullOrEmpty(profileDto.Extension2) ? profileDto.Extension2 : RuleSPTreeUtil.ConvertXmlStrToGoogleTreeStr(profileDto.Extension2);
            }
            else if (profileDto.Type == JobType.TeamsBCSTermUsageReport || profileDto.Type == JobType.TeamsOrphanedTermUsageReport || profileDto.Type == JobType.TeamsRetiredTermUsageReport)
            {
                profileDto.Extension2 = string.IsNullOrEmpty(profileDto.Extension2) ? profileDto.Extension2 : await util.GetFilteredTeamsTreeNodesAsync(SPTreeUtil.ConvertXmlStrToSPTreeJsonStr(profileDto.Extension2), profileDto.Type);
            }
            else
            {
                Logger.Error("profile type error: {0}", profileDto.Type);
            }
            return profileDto;
        }

        [RACodeReview("Allen Yin")]
        [HttpPost]
        [ValidEditReportProfileParameterActionFilter]
        public async Task<string> EditProfile([FromBody]RMProfileDto profile)
        {
            try
            {
                try
                {
                    if (profile.Extension2.IsNullOrEmpty() || profile.ProfileName.IsNullOrEmpty())
                    {
                        throw new ArgumentNullException(nameof(profile));
                    }
                    if ((profile.Type == JobType.FSBCSTermUsageReport || profile.Type == JobType.EXOTermUsageReport
                        || profile.Type == JobType.BCSTermUsageReport || profile.Type == JobType.PhysicalTermUsageReport
                        || profile.Type == JobType.OneDriveTermUsageReport || profile.Type == JobType.SPOnPremBCSTermUsageReport
                        || profile.Type == JobType.BoxBCSTermUsageReport || profile.Type == JobType.GoogleBCSTermUsageReport
                        || profile.Type == JobType.TeamsBCSTermUsageReport) && profile.Extension1.IsNullOrEmpty())
                    {
                        throw new ArgumentNullException(nameof(profile));
                    }
                    if ((int)profile.Type >= 2101 && (int)profile.Type <= 2103)
                    {
                        var EXORoot = SPSettingTreeService.LoadExchangeRoot()[0];
                        profile.Extension2 = string.IsNullOrEmpty(profile.Extension2) ? profile.Extension2 : SPTreeUtil.BuildEXOTreeXMLStr(profile.Extension2, EXORoot.Id);
                    }
                    else if (profile.Type == JobType.BCSTermUsageReport || profile.Type == JobType.OrphanedTermReport || profile.Type == JobType.RetiredTermReport)
                    {
                        var spFarm = SPSettingTreeService.LoadFarm()[0];
                        profile.Extension2 = string.IsNullOrEmpty(profile.Extension2) ? profile.Extension2 : SerializerHelper.SerializeByDataContractSerializer(SerializerHelper.DeserializeByJsonConvert<RMSPTreeNode>(profile.Extension2));
                    }
                    else if (profile.Type == JobType.PhysicalTermUsageReport || profile.Type == JobType.PhysicalOrphanedTermUsageReport || profile.Type == JobType.PhysicalRetiredTermUsageReport)
                    {
                        if (!LocationManagementService.CheckPhysicalRootLocation(profile.Extension2))
                        {
                            throw new Exception("Invalid Physical Root Location.");
                        }
                    }
                    else if (profile.Type == JobType.FSBCSTermUsageReport || profile.Type == JobType.FSOrphanedTermReport || profile.Type == JobType.FSRetiredTermReport)
                    {
                        if (!RMReportService.CheckFSRootNode(profile.Extension2))
                        {
                            throw new Exception("Invalid FS Nodes.");
                        }
                        profile.Extension2 = string.IsNullOrEmpty(profile.Extension2) ? profile.Extension2 : SerializerHelper.SerializeByDataContractSerializer(SerializerHelper.DeserializeByJsonConvert<RMFSTreeNode>(profile.Extension2));
                    }
                    else if (profile.Type == JobType.OneDriveTermUsageReport || profile.Type == JobType.OneDriveOrphanedTermUsageReport || profile.Type == JobType.OneDriveRetiredTermUsageReport)
                    {
                        //var spFarm = mSPSettingTreeService.LoadFarm()[0];
                        profile.Extension2 = string.IsNullOrEmpty(profile.Extension2) ? profile.Extension2 : SerializerHelper.SerializeByDataContractSerializer(SerializerHelper.DeserializeByJsonConvert<RMSPTreeNode>(profile.Extension2));
                    }
                    else if (profile.Type == JobType.SPOnPremBCSTermUsageReport || profile.Type == JobType.SPOnPremOrphanedTermReport || profile.Type == JobType.SPOnPremRetiredTermReport)
                    {
                        profile.Extension2 = string.IsNullOrEmpty(profile.Extension2) ? profile.Extension2 : SPTreeUtil.BuildSPTreeXMLStr(profile.Extension2, "");
                    }
                    else if (profile.Type == JobType.BoxBCSTermUsageReport || profile.Type == JobType.BoxOrphanedTermUsageReport || profile.Type == JobType.BoxRetiredTermUsageReport)
                    {
                        if (!RMReportService.CheckBoxRootNode(profile.Extension2))
                        {
                            throw new Exception("Invalid Box Nodes.");
                        }
                        profile.Extension2 = string.IsNullOrEmpty(profile.Extension2) ? profile.Extension2 : SerializerHelper.SerializeByDataContractSerializer(SerializerHelper.DeserializeByJsonConvert<BoxTreeNode>(profile.Extension2));
                    }
                    else if (profile.Type == JobType.GoogleBCSTermUsageReport || profile.Type == JobType.GoogleOrphanedTermUsageReport || profile.Type == JobType.GoogleRetiredTermUsageReport)
                    {
                        profile.Extension2 = string.IsNullOrEmpty(profile.Extension2) ? profile.Extension2 : SerializerHelper.SerializeByDataContractSerializer(SerializerHelper.DeserializeByJsonConvert<RMGoogleTreeNode>(profile.Extension2));
                    }
                    else if (profile.Type == JobType.TeamsBCSTermUsageReport || profile.Type == JobType.TeamsOrphanedTermUsageReport || profile.Type == JobType.TeamsRetiredTermUsageReport)
                    {
                        profile.Extension2 = string.IsNullOrEmpty(profile.Extension2) ? profile.Extension2 : SerializerHelper.SerializeByDataContractSerializer(SerializerHelper.DeserializeByJsonConvert<RMSPTreeNode>(profile.Extension2));
                    }
                    else
                    {
                        Logger.Error("profile type error: {0}", profile.Type);
                        return "Parameter exception";
                    }
                }
                catch (Exception)
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
            }
            catch (Exception e)
            {
                var returnMessage = string.Empty;
                if (e.Message.Contains("The field Description must be a string or array type with a maximum length of '255'."))
                {
                    returnMessage = I18NEntity.GetString("RM_JS_Profile_Description_TooLong");
                }
                Logger.Info("edit profile failed,name:{0},Type:{1},Error:{2}", profile.ProfileName, profile.Type, e.ToString());
                return returnMessage;
            }
            return string.Empty;
        }

        [RACodeReview("Allen Yin")]
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

        /// <summary>
        /// start BCS Term Usage Report Job
        /// </summary>
        /// <param name="id">profile id</param>
        /// <returns>返回JobId</returns>
        [RACodeReview("Allen Yin")]
        [HttpPost]
        [ValidReportProfileParameterActionFilter]
        public string GenerateReport([FromBody]RMProfileDto profile)
        {
            bool isRetiredTermReport = profile.Type == JobType.RetiredTermReport || profile.Type == JobType.FSRetiredTermReport 
                || profile.Type == JobType.EXORetiredTermUsageReport || profile.Type == JobType.PhysicalRetiredTermUsageReport 
                || profile.Type == JobType.OneDriveRetiredTermUsageReport || profile.Type == JobType.SPOnPremRetiredTermReport 
                || profile.Type == JobType.BoxRetiredTermUsageReport || profile.Type == JobType.GoogleRetiredTermUsageReport
                || profile.Type == JobType.TeamsRetiredTermUsageReport ? true : false;
            bool IsOrphanedTermReport = profile.Type == JobType.OrphanedTermReport || profile.Type == JobType.FSOrphanedTermReport 
                || profile.Type == JobType.EXOOrphanedTermUsageReport  || profile.Type == JobType.PhysicalOrphanedTermUsageReport 
                || profile.Type == JobType.OneDriveOrphanedTermUsageReport || profile.Type == JobType.SPOnPremOrphanedTermReport 
                || profile.Type == JobType.BoxOrphanedTermUsageReport || profile.Type == JobType.GoogleOrphanedTermUsageReport
                || profile.Type == JobType.TeamsOrphanedTermUsageReport ? true : false;
            JobType jobType = (profile.Type == JobType.OrphanedTermReport || profile.Type == JobType.RetiredTermReport)
                                ? JobType.BCSTermUsageReport : profile.Type;
            jobType = (jobType == JobType.EXOOrphanedTermUsageReport || jobType == JobType.EXORetiredTermUsageReport)
                                ? JobType.EXOTermUsageReport : jobType;
            jobType = (jobType == JobType.PhysicalOrphanedTermUsageReport || jobType == JobType.PhysicalRetiredTermUsageReport) ? JobType.PhysicalTermUsageReport : jobType;
            jobType = (jobType == JobType.FSOrphanedTermReport || jobType == JobType.FSRetiredTermReport)
                             ? JobType.FSBCSTermUsageReport : jobType;
            jobType = (jobType == JobType.OneDriveOrphanedTermUsageReport || jobType == JobType.OneDriveRetiredTermUsageReport)
                             ? JobType.OneDriveTermUsageReport : jobType;
            jobType = (jobType == JobType.SPOnPremOrphanedTermReport || jobType == JobType.SPOnPremRetiredTermReport)
                             ? JobType.SPOnPremBCSTermUsageReport : jobType;
            jobType = (jobType == JobType.BoxOrphanedTermUsageReport || jobType == JobType.BoxRetiredTermUsageReport)
                             ? JobType.BoxBCSTermUsageReport : jobType;
            jobType = (jobType == JobType.GoogleOrphanedTermUsageReport || jobType == JobType.GoogleRetiredTermUsageReport)
                             ? JobType.GoogleBCSTermUsageReport : jobType;
            jobType = (jobType == JobType.TeamsOrphanedTermUsageReport || jobType == JobType.TeamsRetiredTermUsageReport)
                             ? JobType.TeamsBCSTermUsageReport : jobType;
            return RMReportService.StartReportJob(jobType, profile.Id, IsOrphanedTermReport, isRetiredTermReport);
        }

        [ValidShowReportQueryPagerActionFilter]
        [HttpPost]
        public Task<string> ShowReportQueryPager([FromBody] ShowReportQuery query)
        {
            return RMReportService.GetCommonReportJobDatasAsync(query);
        }

        [RACodeReview("Allen yin", comment: "没有删除临时文件")]
        [HttpPost]
        //[Microsoft.AspNetCore.Mvc.TypeFilter(typeof(ValidateAntiForgeryTokenFilterAttribute))]
        //[FileDownloadFilter]
        public async Task<IActionResult> DownloadFile()
        {
            string jobId = "";
            string profileName = "";
            string profileId = "";
            jobId = HttpUtility.UrlDecode(Request.Form["jobId"]);
            profileId = HttpUtility.UrlDecode(Request.Form["profileId"]);
            profileName = HttpUtility.UrlDecode(Request.Form["profileName"]);

            HttpResponseMessage response = new HttpResponseMessage();
            BaseJobDto baseJobDto = new BaseJobDto() { Id = jobId, JobType = (int)JobType.BCSTermUsageReport, ProfileName = profileName };
            bool IsOrphanedTermReport = false;
            bool isRetiredTermReport = false;
            if (!string.IsNullOrEmpty(profileId))
            {
                RMProfileDto profile = await RMReportService.GetProfileByIdAsync(profileId);
                IsOrphanedTermReport = profile.Type == JobType.OrphanedTermReport || profile.Type == JobType.OneDriveOrphanedTermUsageReport || profile.Type == JobType.TeamsOrphanedTermUsageReport ? true : false;
                isRetiredTermReport = profile.Type == JobType.RetiredTermReport || profile.Type == JobType.OneDriveRetiredTermUsageReport || profile.Type == JobType.TeamsRetiredTermUsageReport ? true : false;
            }
            await RMReportService.GenerateReportAsync(baseJobDto, IsOrphanedTermReport, isRetiredTermReport);
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
        /// <summary>
        /// 区别与使用TermManagement的方法, 此处不Load Deprecated的Term
        /// </summary>
        /// <param name="tree"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidTermTreeParameterFilter("BrowseTermUsageTermTree")]
        public Task<string> GetAllChildren([FromBody]TreePage tree)
        {
            string nodeId = string.Empty;
            if (tree.NodeId != null)
            {
                nodeId = tree.NodeId;
            }

            string nodeType = string.Empty;
            if (tree.NodeType != null)
            {
                nodeType = tree.NodeType;
            }
            bool needCheckPermission = true;
            return TaxonomyService.GetTaxonomyTreeDataAsync(nodeType, nodeId, false, needCheckPermission);

        }

        [HttpGet]
        public string GetPhysicalSettings()
        {
            var settings = SPSettingTreeService.GetPhysicalInfos();
            return JsonConvert.SerializeObject(settings);
        }
    }
}
