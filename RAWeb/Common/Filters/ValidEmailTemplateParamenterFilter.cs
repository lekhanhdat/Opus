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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Multi_Geo;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.Multi_Geo;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Common.Filters
{
    public class ValidEmailTemplateParamenterFilter : BaseActionFilter
    {

        private static RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public IRMEamilTemplateDao EmailTemplateDao => PlatformWindsorManager.GetService<IRMEamilTemplateDao>();
        private readonly IMultiGeoSettingService MultiGEOSettingService = PlatformWindsorManager.GetService<IMultiGeoSettingService>();
        private IMultiGeoDataCenterService MultiGeoDataCenterService => PlatformWindsorManager.GetService<IMultiGeoDataCenterService>();
        private static string CurrentDCName => RMGlobalConfiguration.AppConfig[RMAppSettingKey.AOS_DATA_CENTER];

        private string action;

        private Dictionary<string, string> FileExtension = new Dictionary<string, string>()
        {
            { "JPEG","255216"},
            { "PNG","13780" },
            { "GIF","7173" },
            { "BMP","6677"},
        };

        private HashSet<string> ImageContentType = new()
        {
            "IMAGE/JPEG",
            "IMAGE/PNG",
            "IMAGE/GIF",
            "IMAGE/BMP"
        };

        private HashSet<string> AllowedTagsTypes = new()
        {
            "a",
            "br",
            "div",
            "em",
            "h1",
            "h2",
            "h3",
            "h4",
            "h5",
            "h6",
            "img",
            "li",
            "ol",
            "s",
            "span",
            "strong",
            "sub",
            "sup",
            "table",
            "tbody",
            "td",
            "tr",
            "u",
            "ul",
            "p"
        };

        public ValidEmailTemplateParamenterFilter()
        {

        }

        public ValidEmailTemplateParamenterFilter(string action)
        {
            this.action = action;
        }

        protected override async Task OnActionAuthenticatedAsync(ActionExecutingContext actionContext)
        {
            if (action.Equals("ValidateUploadImage"))
            {
                if (await MultiGEOSettingService.IsEnableMultiGeoFeature())
                {
                    var mainDC = MultiGeoDataCenterService.GetMainDC();
                    if (mainDC != null && mainDC != CurrentDCName)
                    {
                        actionContext.Result = new ObjectResult("Access Denied") { StatusCode = (int)HttpStatusCode.Forbidden };
                        return;
                    }
                }
 
                if (actionContext.ActionArguments.Values.Count == 0)
                {
                    actionContext.Result = new ObjectResult("Illegal Parameter") { StatusCode = (int)HttpStatusCode.Forbidden };
                    return;
                }
                var fileUp = actionContext.ActionArguments.Values.GetItemByIndex(0) as IFormFile;
                var contentType = fileUp.ContentType.ToUpper();
                if (!ImageContentType.Contains(contentType))
                {
                    actionContext.Result = new ObjectResult("Access Denied") { StatusCode = (int)HttpStatusCode.Forbidden };
                    return;
                }

                var binaryReader = new System.IO.BinaryReader(fileUp.OpenReadStream());
                var fileLength = binaryReader.BaseStream.Length;
                var fileSize = fileLength / (1024 * 1024);
                string bx = " ";
                byte buffer;
                try
                {
                    buffer = binaryReader.ReadByte();
                    bx = buffer.ToString();
                    buffer = binaryReader.ReadByte();
                    bx += buffer.ToString();
                }
                catch (Exception exc)
                {
                    logger.Error(exc.Message);
                }
                var matchedExtension = FileExtension.FirstOrDefault(item => item.Value == bx).Key;
                var canUpload = !string.IsNullOrEmpty(matchedExtension) && ImageContentType.Contains($"IMAGE/{matchedExtension}") && fileSize <= 10;
                binaryReader.Close();
                if (!canUpload)
                {
                    actionContext.Result = new ObjectResult("Access Denied") { StatusCode = (int)HttpStatusCode.Forbidden };
                    return;
                }
            }

            if (action.Equals("ValidateTemplateLength"))
            {
                var emailDto = actionContext.ActionArguments.Values.FirstOrDefault() as EmailTemplateDto;
                if (emailDto == null || emailDto?.Body == null)
                {
                    actionContext.Result = new ObjectResult("Illegal Parameter") { StatusCode = (int)HttpStatusCode.Forbidden };
                    return;
                }
                if (emailDto != null && emailDto.Body.Length == 0)
                {
                    actionContext.Result = new ObjectResult("Access Denied") { StatusCode = (int)HttpStatusCode.Forbidden };
                }

                var body = emailDto.Body;
                var pattern = @"<\s*(\w+)";

                MatchCollection matches = Regex.Matches(body, pattern);
                foreach (Match match in matches)
                {
                    string tag = match.Groups[1].Value.ToLower();
                    if (!AllowedTagsTypes.Contains(tag))
                    {
                        actionContext.Result = new ObjectResult("Illegal Parameter") { StatusCode = (int)HttpStatusCode.Forbidden };
                        return;
                    }
                }
            }
            if (action.Equals("ValidateTemplateGuid"))
            {
                bool isGuid = Guid.TryParse(actionContext.ActionArguments.Values.FirstOrDefault()?.ToString(), out var uniqueId);
                if (!isGuid)
                {
                    actionContext.Result = new ObjectResult("Illegal Parameter") { StatusCode = (int)HttpStatusCode.Forbidden };
                    return;
                }
                if (EmailTemplateDao.GetEmailTemplateByUniqueId(uniqueId) is null)
                {
                    actionContext.Result = new ObjectResult("Illegal Parameter") { StatusCode = (int)HttpStatusCode.Forbidden };
                    return;
                }
            }
        }
    }
}
