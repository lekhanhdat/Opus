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
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.Filters;
using AvePoint.RA.Web.Common.WIF;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;


namespace AvePoint.RA.Web.Controllers.ReportCenter
{
    [RMApiAuthorize(Contract.RoleAssignments.RMPermissionMasks.PhysicalAdmin, preferred: false)]
    public class AvailableSpaceReportApiController : BaseApiController
    {
        private IRMReportService _RMReportService;
        private IRMReportService RMReportService => PlatformWindsorManager.GetService(ref _RMReportService);


        [HttpPost]
        public async Task<string> GetAvailableSpaceReport([FromBody]ShowProfilesReportPageInfo pageInfo)
        {
            ShowProfilesReportPageInfo result = await RMReportService.GetAvailableSpaceReportProfilesAsync(pageInfo);
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
                Type = JobType.AvailableSpaceReport,
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
        [ValidCreateReportProfileParameterActionFilter]
        public async Task<string> CreateProfile([FromBody]RMProfileDto profile)
        {
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
        [ValidReportIdParameterActionFilter]
        public async Task<RMProfileDto> LoadProfileById([FromBody]string Id)
        {
            var profileDto = await RMReportService.GetProfileByIdAsync(Id);
            return profileDto;
        }

        [HttpPost]
        [ValidEditReportProfileParameterActionFilter]
        public async Task<string> EditProfile([FromBody]RMProfileDto profile)
        {
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
        /// start Available Space Report Job
        /// </summary>
        /// <param name="id">profile id</param>
        /// <returns>返回JobId</returns>
        [HttpPost]
        [ValidReportProfileParameterActionFilter]
        public string GenerateReport([FromBody]RMProfileDto profile)
        {
            JobType jobType = profile.Type;
            return RMReportService.StartReportJob(jobType, profile.Id);
        }

        [ValidShowReportQueryPagerActionFilter]
        public Task<string> ShowReportQueryPager([FromBody] ShowReportQuery query)
        {
            return RMReportService.GetCommonReportJobDatasAsync(query);
        }

        [HttpPost]
        //[Microsoft.AspNetCore.Mvc.TypeFilter(typeof(ValidateAntiForgeryTokenFilterAttribute))]
        //[FileDownloadFilter]
        public async Task<IActionResult> DownloadFile()
        {
            string jobId = "";
            string profileName = "";
            string profileId = "";
            var context = this.HttpContext;
            jobId = HttpUtility.UrlDecode(context.Request.Form["jobId"]);
            profileName = HttpUtility.UrlDecode(context.Request.Form["profileName"]);
            profileId = HttpUtility.UrlDecode(context.Request.Form["profileId"]);

            HttpResponseMessage response = new HttpResponseMessage();
            BaseJobDto baseJobDto = new BaseJobDto() { Id = jobId, JobType = (int)JobType.AvailableSpaceReport, ProfileName = profileName };
            bool IsOrphanedTermReport = false;
            if (!string.IsNullOrEmpty(profileId))
            {
                RMProfileDto profile = await RMReportService.GetProfileByIdAsync(profileId);
                IsOrphanedTermReport = false;
            }
            await RMReportService.GenerateReportAsync(baseJobDto, IsOrphanedTermReport);
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

        [HttpPost]
        public string GetAllChildren([FromBody]TreePage tree)
        {
            //string nodeId = string.Empty;
            //if (tree.NodeId != null)
            //{
            //    nodeId = tree.NodeId;
            //}

            //string nodeType = string.Empty;
            //if (tree.NodeType != null)
            //{
            //    nodeType = tree.NodeType;
            //}

            //return TaxonomyService.GetTaxonomyTreeData(nodeType, nodeId, false);
            return "";

        }

    }
}