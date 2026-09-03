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
using AvePoint.GCommon.GraphAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.Wrapper.Common.Graph
{
    public class GroupSiteHelper
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        public static string GetGroupSiteUrlByEmail(string email,AveBPOSAccountInfo accountInfo)
        {
            IGraphTokenProvider provider = GraphTokenProviderFactory.CreateProvider(accountInfo);
           var api= new MicrosoftGraphAPIService(provider.ResourceUrl, provider.GetToken, new GraphLogger());
            var id=api.GetGroupInfoByAddress(email).Id;
            var siteUrl = api.GetGroupSiteByGroupId(id).WebUrl;
            return siteUrl;
        }

        public static IList<GraphUser> GetGroupOwnersById(string id, AveBPOSAccountInfo accountInfo)
        {
            IGraphTokenProvider provider = GraphTokenProviderFactory.CreateProvider(accountInfo);
            var api = new MicrosoftGraphAPIService(provider.ResourceUrl, provider.GetToken, new GraphLogger());
            var users = api.ListGroupOwners(id);
            return users;
        }

        public static bool CheckGroupSiteUrl(string groupEmail, AveBPOSAccountInfo account, ref string outputSiteUrl)
        {
            if (string.IsNullOrEmpty(groupEmail))
            {
                log.Warn("Group email is null or empty.");
                return false;
            }
            var groupSiteUrl = GetGroupSiteUrlByEmail(groupEmail, account);
            if(string.IsNullOrEmpty(groupSiteUrl))
            {
                log.Warn($"The found group site url is null or empty.Email:{groupEmail}");
                return false;
            }
            log.Warn($"Update Group Site Url,Original Url:{outputSiteUrl},new Url:{groupSiteUrl}");
            outputSiteUrl = groupSiteUrl;
            return true;
        }
    }
}
