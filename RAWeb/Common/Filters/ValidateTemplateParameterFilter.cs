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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb.TemplateManagement;
using AvePoint.RA.Contract.RMWeb.TemplateManagement.Barcode;
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
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
    public class ValidateTemplateParameterFilter : BaseActionFilter
    {
        private static ITemplateManagementService TemplateManagementService => PlatformWindsorManager.GetService<ITemplateManagementService>();
        private static RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private string action;

        public ValidateTemplateParameterFilter()
        {

        }
        public ValidateTemplateParameterFilter(string action)
        {
            this.action = action;
        }
        protected override Task OnActionAuthenticatedAsync(ActionExecutingContext actionContext)
        {
            Object parmObj = actionContext.ActionArguments.Values.FirstOrDefault();
            if (action.Equals("ValidateSaveTemplate"))
            {
                if (parmObj is TemplateDto template && template != null)
                {
                    try
                    {
                        if (NeedValidateTemplateUniqueId())
                        { 
                            CheckTemplateUniqueId(new List<string> { template.prefix });
                        }
                        TemplateManagementService.CheckCategoriesAndColumnsData(template);
                    }
                    catch (Exception e)
                    {
                        actionContext.Result = new ObjectResult("Access Denied") { StatusCode = (int)HttpStatusCode.Forbidden };
                    }
                }
            }

            if (action.Equals("ValidateSaveGlobalUniqueId"))
            {
                if (parmObj is GlobalUniqueIdSettingsDto uniqueIdDto && uniqueIdDto != null)
                {
                    try
                    {
                        var prefixList = new List<string>
                        {
                            uniqueIdDto.BoxTemplatePrefix,
                            uniqueIdDto.FolderTemplatePrefix,
                            uniqueIdDto.RecordTemplatePrefix,
                            uniqueIdDto.CustomTemplatePrefix
                        };
                        CheckTemplateUniqueId(prefixList);
                    }
                    catch (Exception)
                    {
                        actionContext.Result = new ObjectResult("Access Denied") { StatusCode = (int)HttpStatusCode.Forbidden };
                    }
                }
            }

            return Task.CompletedTask;
        }

        private bool NeedValidateTemplateUniqueId()
        {
            var result = false;
            try
            {
                var json = TemplateManagementService.LoadingUniqueIdSetting();
                var setting = Newtonsoft.Json.JsonConvert.DeserializeObject<RMPhysicalUniqueIdSetting>(json);
                var enableGlobalUniqueIdSetting = setting != null && setting.IsGlobalSetting;
                result = !enableGlobalUniqueIdSetting;
            }
            catch (Exception ex)
            {
                logger.Error($"An error while NeedValidateTemplateUniqueId, message: {ex}");
            }
            return result;
        }
        private void CheckTemplateUniqueId(List<string> prefixList)
        {
            prefixList.ForEach(p =>
            {
                if (!ValidateUniqueIdPrefix(p))
                {
                    throw new Exception("The template uniqueId prefix is invalid");
                }
            });
        }

        private static bool ValidateUniqueIdPrefix(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }
            var reg = new Regex(@"^[\sA-Za-z0-9\""!#$%&'()*+,./:;<=>?@[\\\]^`{|}~_-]+$", RegexOptions.None, RecordsConstants.REGEX_DEFAULT_MATCH_TIMEOUT);
            return reg.IsMatch(str);
        }
    }


    public class ValidateBarcodeTemplateParameterFilter : BaseActionFilter
    {
        private const int MaxSuiteNameLength = 450;
        private RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private IRMCustomBarcodeTemplateSuiteDao TemplateSuiteDao  => PlatformWindsorManager.GetService<IRMCustomBarcodeTemplateSuiteDao>();
        
        public ValidateBarcodeTemplateParameterFilter() { }
        private string action;

        public ValidateBarcodeTemplateParameterFilter(string action)
        {
            this.action = action;
        }
        protected override Task OnActionAuthenticatedAsync(ActionExecutingContext actionContext)
        {
            if (action.Equals("BatchDeleteCustomBarcodeTemplateSuites"))
            {
                var parmObj = actionContext.ActionArguments.Values.FirstOrDefault();
                if (parmObj is List<Guid> suiteIds && suiteIds != null)
                {
                    if (suiteIds == null || !suiteIds.Any())
                    {
                        actionContext.Result = new ObjectResult("suiteIds can not be null or empty") { StatusCode = (int)HttpStatusCode.BadRequest };
                        return Task.CompletedTask;
                    }

                    var existingSuites = TemplateSuiteDao.GetByUniqueIdsAsync(suiteIds).GetAwaiter().GetResult();
                    if (existingSuites.Any(s => s.IsDefault))
                    {
                        logger.Error($"An error occurred while validating batch delete custom barcode template suites: Can not delete default template");
                        actionContext.Result = new ObjectResult("Can not delete default template") { StatusCode = (int)HttpStatusCode.Forbidden };
                        return Task.CompletedTask;
                    }
                }
            }
            else if (action.Equals("CreateCustomBarcodeTemplateSuites"))
            {
                var parmObj = actionContext.ActionArguments.Values.FirstOrDefault();
                if (parmObj is BarcodeCustomTemplateDto dto && dto != null)
                {
                    if (string.IsNullOrWhiteSpace(dto.Name) || dto.Name.Trim().Length > MaxSuiteNameLength)
                    {
                        actionContext.Result = new ObjectResult($"Name must be between 1 and {MaxSuiteNameLength} characters") { StatusCode = (int)HttpStatusCode.BadRequest };
                        return Task.CompletedTask;
                    }

                    if (dto.Templates == null || !dto.Templates.Any())
                    {
                        actionContext.Result = new ObjectResult("Templates can not be null or empty") { StatusCode = (int)HttpStatusCode.BadRequest };
                        return Task.CompletedTask;
                    }
                }
            }
            else if (action.Equals("UpdateCustomBarcodeTemplateSuites"))
            {
                var parmObj = actionContext.ActionArguments.Values.FirstOrDefault();
                if (parmObj is BarcodeCustomTemplateDto dto && dto != null)
                {
                    if (string.IsNullOrWhiteSpace(dto.Name) || dto.Name.Trim().Length > MaxSuiteNameLength)
                    {
                        actionContext.Result = new ObjectResult($"Name must be between 1 and {MaxSuiteNameLength} characters") { StatusCode = (int)HttpStatusCode.BadRequest };
                        return Task.CompletedTask;
                    }

                    if (dto.Templates == null || !dto.Templates.Any())
                    {
                        actionContext.Result = new ObjectResult("Templates can not be null or empty") { StatusCode = (int)HttpStatusCode.BadRequest };
                        return Task.CompletedTask;
                    }
                }
            }

            return Task.CompletedTask;
        }

    }
}