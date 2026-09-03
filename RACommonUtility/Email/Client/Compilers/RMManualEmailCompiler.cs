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
using AvePoint.RA.Common;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Email.Client.Config;
using AvePoint.RA.RACommonUtility.Email.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using Microsoft.Extensions.FileSystemGlobbing.Internal.Patterns;
using PnP.Framework.Modernization.Extensions;
using Simple.OData.Client.Extensions;
using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace AvePoint.RA.RACommonUtility.Email.Client.Compilers
{
   
    public class RMManualEmailCompiler : IRMEmailCompiler
    {
        public RMEmailTemplateType TemplateType => RMEmailTemplateType.Manual;

        private static IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();
           
        public static readonly string patternCurrentDate = "\\$Current\\.Date(\\+[1-9][0-9]*)?\\$";

        public static readonly string patternCurrentDateNumber = "[0-9]+";
        public string CompileBody(string body, RMEmailTemplateParameters parameters)
        {
            var manualParameters = parameters as RMManualEmailTemplateParameters;

            body = ReplacePlaceholders(body, parameters);

            var link = RMEmailTemplateHtml.MANUAL_REVIEW_LINK.Replace("@Link", manualParameters.RequestLink);
            link = link.Replace("@Automation", I18NEntity.GetString("RM_JS_Common_RecourdAutomation"));
            link = link.Replace("@Title", manualParameters.RequestLinkTitle);

            body = body.Replace($"$Request.Link$", link);

            return body;
        }

        public string CompileSubject(string subject, RMEmailTemplateParameters parameters)
        {
            return ReplacePlaceholders(subject, parameters);
        }

        private static string ReplacePlaceholders(string content, RMEmailTemplateParameters parameters)
        {
            var matches = Regex.Matches(content, patternCurrentDate);

            var manualParameters = parameters as RMManualEmailTemplateParameters;
            var propertyInfoList = manualParameters.GetType().GetProperties();

            foreach (var propertyInfo in propertyInfoList)
            {

                var placeholder = propertyInfo.GetAttribute<RMEmailTemplatePlaceholderAttribute>()?.PlaceHolder;
                var value = propertyInfo.GetValue(manualParameters)?.ToString() ?? "";

                if (!string.IsNullOrEmpty(placeholder))
                {
                    if (placeholder.Equals("$Current.Date$") && matches.Count > 0) 
                    {
                        foreach (Match match in matches.Cast<Match>())
                        {
                            var stringDateNumber = Regex.Match(match.Value, patternCurrentDateNumber);
                            if (stringDateNumber.Success) 
                            {
                                var intDateNumber = int.Parse(stringDateNumber.Value);
                                if (intDateNumber <= 10000) 
                                {
                                    var addDateValue = GeneralSettingService.ConvertTiksToDateNoTime(GeneralSettingService.GetGeneralSettingAsync().GetAwaiter().GetResult(), DateTimeOffset.Now.AddDays(intDateNumber).Ticks);
                                    content = content.Replace(match.Value, addDateValue);
                                }
                            } 
                        }
                        
                    }
                    content = content.Replace(placeholder, value);
                }
            
            }

            return content;
        }
    }
}
