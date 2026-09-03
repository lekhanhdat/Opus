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
using AvePoint.RA.Web.Extentions.Util;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SforceService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace AvePoint.RA.Web.Common.Utils
{

    public class ApiMessageUtil
    {
        protected static RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

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

        public static void SetResponseErrorMsg(RestStateCode errorCode, Exception ex)
        {
            logger.Error("An error occur on executing api: {0}.", ex.ToString());
            SetResponseErrorMsg(errorCode, ex.Message);
        }

        public static void SetResponseErrorMsg(RestStateCode errorCode, string errorMsg = null)
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
            string content = JsonConvert.SerializeObject(account);
            HttpContextExtensions.CurrentHttpContext().Response.WriteAsync(content).GetAwaiter().GetResult();
        }
        public static List<string> GetArchiverImportSitesUrl(IFormFile file)
        {
            List<string> result = new List<string>();
            try
            {
                using (StreamReader reader = new StreamReader(file.OpenReadStream()))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        string[] fields = line?.Split(',');
                        string temp = string.Empty;
                        foreach (string field in fields)
                        {
                            temp = temp + field;
                        }
                        logger.Info($"Current Archiver Import file include field:{temp}.");
                        if (temp.Equals("Site Collection URL",StringComparison.OrdinalIgnoreCase))
                        {
                            logger.Info("This archive import field is Site Collection URL,skip it");
                            continue;
                        }
                        result.Add(temp.Trim('/'));
                    }
                }
                return result;
            }
            catch (Exception e)
            {
                logger.Error($"something went wrong when read sites url from archiver import file,error:{e.ToString()}");
                return null;
            }
        }

        public static List<string> GetArchiverImportTeamsEmailAddress(IFormFile file)
        {
            List<string> result = new List<string>();
            try
            {
                using (StreamReader reader = new StreamReader(file.OpenReadStream()))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        string[] fields = line?.Split(',');
                        string temp = string.Empty;
                        foreach (string field in fields)
                        {
                            temp = temp + field;
                        }
                        logger.Info($"Current Teams Archiver Import file include field:{temp}.");
                        if (temp.Equals("Team Email Address", StringComparison.OrdinalIgnoreCase))
                        {
                            logger.Info("This archive import field is Team Email Address,skip it");
                            continue;
                        }
                        result.Add(temp);
                    }
                }
                return result;
            }
            catch (Exception e)
            {
                logger.Error($"something went wrong when read Team Email Address from Teams archiver import file,error:{e.ToString()}");
                return null;
            }
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