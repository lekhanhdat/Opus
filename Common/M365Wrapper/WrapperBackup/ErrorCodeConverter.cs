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

namespace ExchangeUtility.Graph
{
    using AvePoint.GCommon.GraphAPI;
    using AvePoint.Wrapper.Common;
    using AvePoint.Wrapper.Resource;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ErrorCodeConverter
    {
        private static readonly Dictionary<string, string> ErrorCodeConverterDic = new Dictionary<string, string>()
        {
           { "MaximumPlannerPlans","Agent.Office365Group.MaximumPlannerPlans_ADDA364A-C7FF-4316-B9A7-7F3933ABFE3D"},
           { "MaximumProjectsOwnedByUser","Agent.Office365Group.MaximumPlannerPlans_ADDA364A-C7FF-4316-B9A7-7F3933ABFE3D"},
           { "ErrorSendAsDenied","Agent.Planner.RestoreTaskCommentsNolicense_46FD00F4-A530-4E05-847C-C3EC295E7A86" },
        };
        private static readonly Dictionary<string, string> ErrorMessageConverterDic = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
           { "You do not have the required permissions to access this item.",$"Agent.Office365Group.NoPermissionsAccessItem_0CA9D403-ADB9-4348-9F58-6F84B6472333" },
           { "You do not have the required permissions to access this item, or the item may not exist.",$"Agent.Office365Group.NoPermissionsAccessItem_0CA9D403-ADB9-4348-9F58-6F84B6472333" },
        };
        private static readonly Dictionary<String, DynamicDataKey[]> I18NParameterDic = new Dictionary<string, DynamicDataKey[]>(StringComparer.OrdinalIgnoreCase)
        {
            { "Agent.Office365Group.NoPermissionsAccessItem_0CA9D403-ADB9-4348-9F58-6F84B6472333", new DynamicDataKey[]{ DynamicDataKey.PlannerUserName} },
            { "Agent.Planner.RestoreTaskCommentsNolicense_46FD00F4-A530-4E05-847C-C3EC295E7A86", new DynamicDataKey[]{ DynamicDataKey.PlannerUserName} },
        };

        public static string GraphAPIErrorCodeConverter(GraphAPIException ex, I18NParameterCollector parameterCollector)
        {
            var get = ErrorCodeConverterDic.TryGetValue(ex.Error.Code, out var message) || ErrorMessageConverterDic.TryGetValue(ex.Error.Message, out message);
            if (!get)
            {
                if (ex.Error.Message.Contains("403 - Forbidden: Access is denied") && ex.Error.Message.Contains("You do not have permission to view this directory or page using the credentials that you supplied"))
                {
                    message = "Agent.Office365Group.VisitPlannerFailed_072E474D-101F-4EA6-A526-B767871A8600";
                    get = true;
                }
                else if (ex.Error.Message.Contains("404 - File or directory not found.") && ex.Error.Message.Contains("The resource you are looking for might have been removed, had its name changed, or is temporarily unavailable."))
                {
                    message = "Agent.Office365Group.TemporarilyUnavailable_575956E5-1C1C-420D-840B-91896F037EA4";
                    get = true;
                }
                else if (ex.Error.Message.Contains("Referenced User") && ex.Error.Message.Contains("is not found"))
                {
                    message = "Agent.Office365Group.AssignUserNotFound_2317A0B1-822D-01DE-9522-07B7A84429A2";
                    get = true;
                }
                else if (ex.HttpStatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                {
                    message = "Agent.Office365Group.PlannerServiceUnavailable_DEE2E3FB-F64E-B7E6-5B4B-95EFDF816F06";
                    get = true;
                }
                else if (ex.Message.Contains("Tenant is not found"))
                {
                    message = "Agent.Office365Group.TenantNotFound_FB9DC968-002B-4ACD-A16F-4AEFE9C2FD0A";
                    get = true;
                }
            }
            return get ? AddI18NParameters(message, parameterCollector) : $"{ex.Error.Message}, ErrorCode: {ex.Error.Code}, HttpStatusCode: {ex.HttpStatusCode}.";
        }

        public static ServiceError GetErrorCode(string code)
        {
            var errorCode = (ServiceError)(0);
            if (!string.IsNullOrEmpty(code)) _ = Enum.TryParse(code, out errorCode);
            return errorCode;
        }
        public static String AddI18NParameters(String i18nKey, I18NParameterCollector parameterCollector)
        {
            if (I18NParameterDic.TryGetValue(i18nKey, out DynamicDataKey[] dataKeys))
            {
                if (null == parameterCollector) return i18nKey;
                var parameters = dataKeys.Select(datakey => parameterCollector.GetData(datakey)).ToArray();
                return ExchangeReportMessage.CreateReportMessage(i18nKey, parameters);
            }
            return i18nKey;
        }
    }

    public enum DynamicDataKey
    {
        Unknown = 0,
        GroupId = 1,
        UserName = 2,
        PlannerUserName,
    }

    public static class ExceptionConverter
    {
        public static string ExtractAggregateExceptionMessage(this Exception ex)
        {
            if (ex == null) return string.Empty;
            if (ex.InnerException == null) return ex.Message;
            var innerException = ex.InnerException;
            if (innerException.Message != "One or more errors occurred.") return innerException.Message;
            return ExtractAggregateExceptionMessage(innerException);
        }

        public static string WrapAggregateErrorMessage(this Exception ex, string username)
        {
            var errorMsg = ex.ExtractAggregateExceptionMessage();
            var r = System.Text.RegularExpressions.Regex.Match(errorMsg, @"^AADSTS\d+(?=:)");
            if (!r.Success)
            {
                return errorMsg;
            }
            switch (r.Value)
            {
                case "AADSTS50076":
                case "AADSTS50079":
                    return ExchangeReportMessage.CreateReportMessage("Agent.Exchange.MFA_DBC3DD47-C31A-4F59-B252-FEC624CAFB14", username);
                case "AADSTS50126":
                    return ExchangeReportMessage.CreateReportMessage("Wrapper_IncorrectUserNameOrPasswordError", username);
                case "AADSTS65001":
                    //return ""; studo:return AveWrapperHandleErrorMessage.ConvertErrorMessageToXML(WrapperReportResourceKey.Wrapper_NoDelegateApp.ToString(), WrapperReportResource.Wrapper_NoDelegateApp);
                default:
                    return errorMsg;
            }
        }
    }
    public enum KnownLoginError
    {
        Unknown,
        /// <summary>
        /// multi-factor authentication
        /// </summary>
        AADSTS50076,
        /// <summary>
        /// multi-factor authentication
        /// </summary>
        AADSTS50079,
        /// <summary>
        /// validating credentials
        /// </summary>
        AADSTS50126,
        /// <summary>
        /// Conditional Access policies
        /// </summary>
        AADSTS53003,
    }

}