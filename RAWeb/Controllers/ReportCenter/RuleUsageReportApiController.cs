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
using AvePoint.GCommon;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.CodeView;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.RuleUsageReport;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.WIF;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Controllers.ReportCenter
{
    [RACodeReview("Allen yin")]
    [RMApiAuthorize(RMPermissionMasks.ReportCenterEnduser, RMReportPermissionMasks.RuleUsageEnduser, preferred: false)]
    public class RuleUsageReportApiController : BaseApiController
    {
        private IRuleManagerService _RuleManager;
        private IRuleManagerService RuleManager => PlatformWindsorManager.GetService(ref _RuleManager);
        private IRMRuleUsageReportService _RuleUsageReportService;
        private IRMRuleUsageReportService RuleUsageReportService => PlatformWindsorManager.GetService(ref _RuleUsageReportService);
        private IGeneralSettingService _GeneralSettingService;
        private IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService(ref _GeneralSettingService);

        /// <summary>
        /// 获取RA rule关联的Term信息
        /// </summary>
        /// <param name="RuleId"></param>
        /// <param name="RuleName"></param>
        /// <returns></returns>
        [RACodeReview("Allen yin")]
        [HttpGet]
        public async Task<string> GetRuleUsageInfo(string RuleId, string RuleName)
        {
            List<RuleUsageInfo> RuleUsageInfo = await RuleUsageReportService.GetRuleUsageInfoByRuleIdAsync(RuleId, RuleName);
            return JsonConvert.SerializeObject(RuleUsageInfo);
        }
        /// <summary>
        /// 获取所有RA Rule
        /// </summary>
        /// <returns></returns>
        [RACodeReview("Allen yin")]
        [HttpPost]
        public async Task<string> GetRuleInfo()
        {
            List<RMRuleInfos> RuleInfo = await RuleManager.GetSimpleRecordsRulesFromDBAsync();
            return JsonConvert.SerializeObject(RuleInfo);
        }

        /// <summary>
        /// download load Report
        /// </summary>
        /// <param name="ruleId"></param>
        /// <returns></returns>
        [RACodeReview("Allen Yin")]
        [HttpGet]
        public async Task<IActionResult> DownLoadReport()
        {
            string ruleId = "";
            ruleId = Request.Query["ruleId"].ToString();

            HttpResponseMessage response = new HttpResponseMessage();
            DateTime nowTime = DateTime.UtcNow;
            string nowTimeStr = (await GeneralSettingService.ConvertTiksToDateTimeAsync(nowTime.Ticks, false)).DataTime.ToString(AveDateTimeUtility.DATETYPE022);
            string fileName = I18NEntity.GetString("RM_RC_RUR_PageTitle") + "_" + nowTimeStr;
            string folderPath = JobReportUtility.GetDownloadRuleUsageReportTempleFolder("Temple") + Path.DirectorySeparatorChar + fileName + Guid.NewGuid();
            string reportFilePath = folderPath + Path.DirectorySeparatorChar + fileName + ".xlsx";
            await RuleManager.GenerateReportForRuleReportAsync(folderPath, fileName, I18NEntity.GetString("RM_RC_RUR_RuleDetailTitle"), ruleId);
            await RuleUsageReportService.GenerateReportForRuleUsageReportAsync(reportFilePath, ruleId, I18NEntity.GetString("RM_RC_RUR_TermDetail"));
            ZipUtil.ZipFolder(folderPath, folderPath + ".zip", Encoding.UTF8);
            var memoryStream = new MemoryStream();
            using (var stream = new FileStream(folderPath + ".zip", FileMode.Open, FileAccess.Read))
            {
                stream.CopyTo(memoryStream);
            }
            memoryStream.Position = 0;
            return File(memoryStream, GetContentType(folderPath + ".zip"), fileName + ".zip");
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