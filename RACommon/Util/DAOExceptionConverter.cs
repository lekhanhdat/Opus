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
using AvePoint.RA.Contract.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Common.Util
{
    public class DAOExceptionConverter
    {
        private static RALogger logger = RALogger.GetInstance(typeof(DAOExceptionConverter));
        private const string RESPONSEMESSAGE = "response message: ";
        public static string GetExceptionMsg(string errorMessage)
        {
            string result = string.Empty;
            try
            {
                result = GetMessage(errorMessage);
            }
            catch (Exception ex)
            {
                logger.Error("error while get dao exception:{0}", ex.ToString());

            }
            return result;
        }

        private static string GetDAOMessage(string message)
        {
            string result = string.Empty;
            try
            {
                var error = JsonUtil.JsonDeserialize<DAOException>(message);
                result = error.ErrorMessage;
                if (error.ErrorCode == "243")
                {
                    result = error.ErrorCode;
                }
                else if (result.Contains(RESPONSEMESSAGE))
                {
                    var temp = result.Substring(result.IndexOf(RESPONSEMESSAGE) + RESPONSEMESSAGE.Length);
                    result = temp.Substring(0, temp.LastIndexOf("}") + 1);
                }

            }
            catch (Exception ex)
            {
                logger.Warn("Get dao api message error message:{0}, {1}", message, ex.ToString());
            }
            return result;
        }

        private static string GetMessage(string message)
        {
            string result = string.Empty;
            try
            {
                var dbpoMessage = JsonUtil.JsonDeserialize<BaseDBPOException>(message);
                switch (dbpoMessage.type)
                {
                    case "DataSourceException":
                        try
                        {
                            result = GetDAOMessage(dbpoMessage.message);
                        }
                        catch (Exception)
                        {
                            result = dbpoMessage.message;
                        }

                        break;
                    case "DBPOException":
                        result = dbpoMessage.message;
                        break;

                    default:
                        logger.Error("return dbpo message type not found,type:{0},message:{1}", dbpoMessage.type, result);
                        result = GetDAOMessage(message);
                        break;
                }
            }
            catch (Exception ex)
            {
                logger.Warn("Get dbpo api message error message:{0}, {1}", message, ex.ToString());
                result = GetDAOMessage(message);
            }
            return result;
        }

    }
}
