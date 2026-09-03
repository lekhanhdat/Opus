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
using AvePoint.GCommon.Contract.ReportCenter.Object;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using RAReportCenter.ClientAuditReport.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace RAReportCenter.ClientAuditReport.Scanner
{
    internal class ClientAuditReportFilterEngine
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(SharePointOnlineAuditReportScanner));
        ClientAuditReportDto mClientAuditReportDto;
        DateTime mStartTime;
        DateTime mEndTime;

        public ClientAuditReportFilterEngine(ClientAuditReportDto clientAuditReportDto, DateTime start, DateTime end)
        {
            mClientAuditReportDto = clientAuditReportDto;
            mStartTime = start;
            mEndTime= end;
        }

        public bool IsQualified(ClientSPAuditReport data)
        {
            bool match = false;
            try
            {
                match = OccurredTimeFilter.IsQualified(new DateTime(data.Occurred, DateTimeKind.Utc), mStartTime, mEndTime);
                if (!match) return false;

                if (mClientAuditReportDto.ObjType != (int)ClientAuditObjType.All)
                {
                    match = TypeFilter.IsQualified(mClientAuditReportDto.ObjType, data.ObjectLevel);
                    if (!match) return false;
                }

                if (mClientAuditReportDto.ActionType != (int)AuditEventType.All)
                {
                    match = ActionFilter.IsQualified(mClientAuditReportDto.ActionType, data.Event);
                    if (!match) return false;
                }

                if (mClientAuditReportDto.UserScope == UserScopeSettings.SpecificUsers)
                {
                    match = false;
                    bool hasGetUser = false;
                    foreach (var user in mClientAuditReportDto.userInfos)
                    {
                        hasGetUser = UserFilter.IsQualified(user.Email, data.User)
                            || UserFilter.IsQualified(user.UserPrincipalName, data.User);
                        if (hasGetUser)
                        {
                            match = true;
                            break;
                        }
                    }
                    if (!match) return false;
                }

                if (!string.IsNullOrEmpty(mClientAuditReportDto.FilterStr))
                {
                    match = UrlFilter.IsQualified(mClientAuditReportDto.FilterStr, data.Url);
                    if (match) return true;
                    else return false;
                }

                return true;
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while filtering Audit data, error is {e}");
                return false;
            }
        }
    }

    internal class OccurredTimeFilter
    {
        internal static bool IsQualified(DateTime objectValue, DateTime startTime,DateTime endTime )
        {
            return ConditionChecker.Between(objectValue, startTime, endTime);
        }
    }

    internal class UrlFilter
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(UrlFilter));
        internal static bool IsQualified(string criteria, string value)
        {
            bool isMatch = false;
            bool ignoreCase = true;
            if (ignoreCase)
            {
                value = value.ToLowerInvariant();
                criteria = criteria.ToLowerInvariant();
            }
            isMatch = value.Contains(criteria);
            if (isMatch) return true;

            Regex regexRule = null;
            try
            {
                regexRule = new Regex(criteria, RegexOptions.IgnoreCase, TimeSpan.FromSeconds(8));
                isMatch = regexRule.IsMatch(value);
                if (isMatch) return true;
            }
            catch(Exception e)
            {
                Logger.Error($"some thing went wrong when UrlFilter IsQualified,error:{e}");
            }
            try
            {
                criteria = RegexUtility.ConvertWildcardPatternToRegex(criteria);
                regexRule = new Regex(criteria, RegexOptions.None, TimeSpan.FromSeconds(8));
                isMatch = regexRule.IsMatch(value);
                if (isMatch) return true;
            }
            catch(Exception e)
            {
                Logger.Error($"some thing went wrong when UrlFilter IsQualified2,error:{e}");

            }
            return false;
        }
    }

    internal class UserFilter
    {
        internal static bool IsQualified(string criteria, string userName2)
        {
            if (!string.IsNullOrEmpty(criteria))
            {
                return criteria.Equals(userName2, StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                return false;
            }
        }
    }

    internal class ActionFilter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mark">Calculate by AuditEventType enum</param>
        /// <param name="type">AuditEventType enum</param>
        /// <returns></returns>
        internal static bool IsQualified(int criteria, int type)
        {
            return (criteria & type) == type;
        }
    }

    internal class TypeFilter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mark">Calculate by AuditItemType enum</param>
        /// <param name="type">AuditItemType enum</param>
        /// <returns></returns>
        internal static bool IsQualified(int criteria, int type)
        {
            return (criteria & type) == type;
        }
    }

}
