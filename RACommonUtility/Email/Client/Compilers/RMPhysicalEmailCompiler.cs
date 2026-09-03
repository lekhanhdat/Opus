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
using AvePoint.RA.Common.Security;
using AvePoint.RA.RACommonUtility.Email.Client.Config;
using AvePoint.RA.RACommonUtility.Email.Model;
using AvePoint.Wrapper.Common;
using System;
using System.Reflection;

namespace AvePoint.RA.RACommonUtility.Email.Client.Compilers
{
    public class RMPhysicalEmailCompiler : IRMEmailCompiler
    {
        public RMEmailTemplateType TemplateType => RMEmailTemplateType.Physical;

        public string CompileBody(string body, RMEmailTemplateParameters parameters)
        {
            const string requestForReviewPlaceholder = "AvePoint Cloud Records &#62; My Tasks &#62; Requests for Review";
            const string requestManagementPlaceholder = "AvePoint Cloud Records &#62; Request Management";
            const string opusRequestForReviewPlaceholder = "AvePoint Opus &#62; My Tasks &#62; Requests for Review";

            var linkedUrl = AveUrlUtility.CombineUrl(RMSSOHelper.RecoSsoLoginUrl, "?redirect=Root/PRM/MyRequest");

            body = ReplacePlaceholders(body, parameters);
            body = body.Replace(requestForReviewPlaceholder, RMEmailTemplateHtml.PHYSICAL_REQUEST_REVIEW_LINK.Replace("@Link", linkedUrl));
            body = body.Replace(requestManagementPlaceholder, RMEmailTemplateHtml.PHYSICAL_REQUEST_MANAGEMENT_LINK.Replace("@Link", linkedUrl));
            body = body.Replace(opusRequestForReviewPlaceholder, RMEmailTemplateHtml.PHYSICAL_REQUEST_MANAGEMENT_LINK_OPUS.Replace("@Link", linkedUrl));

            return body;
        }

        public string CompileSubject(string subject, RMEmailTemplateParameters parameters)
        {
            return ReplacePlaceholders(subject, parameters);
        }

        private static string ReplacePlaceholders(string content, RMEmailTemplateParameters parameters)
        {
            var physicalParameters = parameters as RMPhysicalEmailTemplateParameters;
            var propertyInfoList = physicalParameters.GetType().GetProperties();
            foreach (var propertyInfo in propertyInfoList)
            {
                var placeholder = propertyInfo.GetAttribute<RMEmailTemplatePlaceholderAttribute>()?.PlaceHolder;
                var value = propertyInfo.GetValue(physicalParameters)?.ToString();
                if (!string.IsNullOrEmpty(placeholder) && !string.IsNullOrEmpty(value))
                {
                    content = content.Replace(placeholder, value);
                }
            }

            return content;
        }
    }
}
