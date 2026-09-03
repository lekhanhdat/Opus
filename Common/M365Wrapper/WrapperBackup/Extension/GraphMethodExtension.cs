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

    using ExchangeCommonWrapper;

    using AvePoint.RA.CommonUtil;

    using System;
    using System.Linq;
    using System.Text;

    public static class GraphMethodExtension
    {
        private static RALogger logger = RALogger.GetInstance(typeof(GraphMethodExtension));

        #region GetLicensedUser
        /// <summary>
        /// 使用黑名单
        /// </summary>
        public static GraphUser GetLicensedUser(this MicrosoftGraphAPIService msGraphAPIService, String groupId, Boolean findowner, Boolean thrownException = true)
        {
            try
            {
                var users = msGraphAPIService.ListGroupUsers(groupId, findowner);
                logger.Info("{0} count in the group : {1}", findowner ? "owners" : "members", users?.Count);
                var firstUser = users.FirstOrDefault(user => IsLicensedUser(user, true));
                if (null != firstUser)
                {
                    RecordUserLicense(firstUser);
                }
                return firstUser;
            }
            catch (Exception ex)
            {
                logger.Warn("An error occurred while getting group first licensed {0}. Reason: {1}. Need thrown : {2}", findowner ? "owner" : "member", ex.ToString(), thrownException);
                if (thrownException) throw;
                return null;
            }
        }
        /// <summary>
        /// 使用白名单
        /// </summary>
        public static GraphUser GetLicensedUser(this MicrosoftGraphAPIService msGraphAPIService, String groupId, FindRole role, Boolean thrownException = true)
        {
            try
            {
                var users = msGraphAPIService.ListGroupUsers(groupId, role == FindRole.owner);
                logger.Info("{0}s count in the group : {1}", role, users?.Count);
                var firstUser = users.FirstOrDefault(user => IsLicensedUser(user));
                if (null != firstUser)
                {
                    RecordUserLicense(firstUser);
                }
                return firstUser;
            }
            catch (Exception ex)
            {
                logger.Warn("An error occurred while getting group first licensed {0}. Reason: {1}. Need thrown : {2}", role, ex.ToString(), thrownException);
                if (thrownException) throw;
                return null;
            }
        }
        private static bool IsLicensedUser(GraphUser user, Boolean enableBlackList = false)
        {
            return user?.AssignedPlans?.Any(asdPlan => IsTargetServer(asdPlan, enableBlackList)) ?? false;
        }
        private static bool IsTargetServer(AssignedPlan assignedPlan, Boolean enableBlackList = false)
        {
            if (ExchangeConstants.LicenseIdDic.TryGetValue(assignedPlan.ServicePlanId, out Boolean isWhitList))
            {
                if (isWhitList) return "Enabled".Equals(assignedPlan.CapabilityStatus, StringComparison.OrdinalIgnoreCase);
                return false;
            }
            else
            {
                if (enableBlackList)
                {
                    return assignedPlan.Service.Equals("exchange", StringComparison.OrdinalIgnoreCase)
                  && assignedPlan.CapabilityStatus.Equals("Enabled", StringComparison.OrdinalIgnoreCase);
                }
                return false;
            }
        }
        private static void RecordUserLicense(GraphUser user)
        {
            var sb = new StringBuilder($"The licsence of user [{user.UserPrincipalName}]:\r\n");
            foreach (var ap in user.AssignedPlans)
            {
                if ("exchange".Equals(ap.Service, StringComparison.OrdinalIgnoreCase)) sb.AppendFormat("[{0},{1},{2},{3}]\r\n", ap.ServicePlanId, ap.AssignedDateTime, ap.CapabilityStatus, ap.Service);
            }
            logger.Info(sb.ToString());
        }

        public enum FindRole
        {
            owner,
            member
        }
        #endregion

        //public static Boolean CheckUserExchangeLicense(this MicrosoftGraphAPIService msGraphAPIService, String userPrincipalName)
        //{
        //    var user = msGraphAPIService.GetUserWithBetaApi(ODataSpecialCharactersConverter.ConvertToS(userPrincipalName));
        //    var isLicenseUser = null != user.AssignedPlans?.Find(asdPlan => IsTargetServer(asdPlan));
        //    if (isLicenseUser)
        //    {
        //        RecordUserLicense(user);
        //    }
        //    return isLicenseUser;
        //}
    }

    public static class TeamChannelExtension
    {
        public static bool IsPrivateChannel(this TeamChannel channel)
        {
            return channel.MembershipType?.Equals("private", StringComparison.OrdinalIgnoreCase) ?? false;
        }
        public static bool IsSharedChannel(this TeamChannel channel)
        {
            return channel.MembershipType?.Equals("shared", StringComparison.OrdinalIgnoreCase) ?? false;
        }
        public static bool IsStandardChannel(this TeamChannel channel)
        {
            return channel.MembershipType?.Equals("standard", StringComparison.OrdinalIgnoreCase) ?? false;
        }
        
    }
}