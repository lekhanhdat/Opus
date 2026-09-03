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
using AvePoint.RA.CommonUtil;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace AvePoint.RA.Web.Common.Utils
{

    public static class ApiMessageUtil
    {
        private static RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public static Dictionary<RestStateCode, string> ErrorMessageDic = new Dictionary<RestStateCode, string>()
        {
            //Login
            { RestStateCode.LoginAutherized, "Login authorized failed: {0}"},
            { RestStateCode.Login, "Login Records failed: {0}"},
            //Term
            { RestStateCode.GetTermColumnInfo, "Get term column Info failed: {0}"},
            { RestStateCode.GetTermTreeByPager, "Get term tree failed: {0}"},
            { RestStateCode.GetTermTreeByEmail, "Get term tree by email failed: {0}"},

        };

        public static void SetResponseErrorMsg(this HttpResponse response, RestStateCode errorCode, Exception ex)
        {
            logger.Error("An error occur on executing api: {0}.", ex.ToString());
            SetResponseErrorMsg(response, errorCode, ex.Message);
        }

        private static void SetResponseErrorMsg(HttpResponse response, RestStateCode errorCode, string errorMsg = null)
        {
            var message = string.Empty;
            if (ErrorMessageDic.ContainsKey(errorCode))
            {
                message = string.Format(ErrorMessageDic[errorCode], errorMsg);
            }
            else
            {
                message = string.Format("An error occur:{0}", errorMsg);
            }
            logger.Error(message);
            JObject account = new JObject
            {
                new JProperty("ErrorCode", (int)errorCode),
                new JProperty("ErrorMessage", message),
            };
            response.WriteAsJsonAsync(account).GetAwaiter().GetResult();
        }
    }

    public enum RestStateCode
    {
        Unknown = 0,
        #region ==Login==
        LoginAutherized = 101,
        Login = 102,
        LicenseExpired = 104,
        #endregion

        #region ==Term Management==   
        GetTermTreeByPager = 201,
        GetTermColumnInfo = 202,
        GetTermTreeByEmail = 203,
        #endregion
    }
}
