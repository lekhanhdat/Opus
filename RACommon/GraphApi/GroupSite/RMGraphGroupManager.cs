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
using AvePoint.GCommon.GraphAPI;
using AvePoint.RA.Common.GraphApi.Mail;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.GraphApi;
using Cloud.Sdk.Data.AosModern;
using DocumentFormat.OpenXml.Office2010.Excel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
//using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using static AvePoint.GCommon.Utility.I18N.EventIds.Configuration;
using ListGroupsObj = AvePoint.RA.Common.GraphApi.Mail.ListGroupsObj;

namespace AvePoint.RA.Common.GraphApi.GroupSite
{
    public class RMGraphGroupManager : RMGraphApiManager
    {
        public RMGraphGroupManager(AppProfileInfo profile) : base(profile)
        { }
        public async Task<RMGraphGroupSiteUrl> GetGroupSite(string groupId)
        {
            var requestUri = $"{GraphEndPoint}/{ApiVersion}/groups/{groupId}/sites/root?$select=webUrl";

            var resultJson = await HttpHelper.GetAsync(requestUri, AccessToken);
            var result = JsonConvert.DeserializeObject<RMGraphGroupSiteUrl>(resultJson);
            return result;

        }
        public async Task<RMGraphGroupSiteUrl> GetSiteById(string siteId)
        {
            var requestUri = $"{GraphEndPoint}/{ApiVersion}/sites/{siteId}";

            var resultJson = await HttpHelper.GetAsync(requestUri, AccessToken);
            var result = JsonConvert.DeserializeObject<RMGraphGroupSiteUrl>(resultJson);
            return result;

        }
        public async Task<RMGroup> GetGroup(string groupId)
        {
            var requestUri = $"{GraphEndPoint}/{ApiVersion}/groups/{groupId}";

            var resultJson = await HttpHelper.GetAsync(requestUri, AccessToken);
            var result = JsonConvert.DeserializeObject<RMGroup>(resultJson);
            return result;

        }

        public async Task<ListGroupsObj> GetUserOwnedObject(string userId, string nextLink = "")
        {
            string requestUri = string.Empty;
            if (string.IsNullOrEmpty(nextLink))
            {
                requestUri = $"{GraphEndPoint}/{ApiVersion}/users/{userId}/ownedObjects";
            }
            else
            {
                requestUri = nextLink;
            }
            var resultJson = await HttpHelper.GetAsync(requestUri, AccessToken);
            var result = JsonConvert.DeserializeObject<ListGroupsObj>(resultJson);
            return result;
        }
        public async Task<ListGroupsObj> GetUserMemberOfGroups(string userId,string nextLink="")
        {
            string requestUri = string.Empty;
            if (string.IsNullOrEmpty(nextLink))
            {
                requestUri = $"{GraphEndPoint}/{ApiVersion}/users/{userId}/memberOf";
            }
            else
            {
                requestUri = nextLink;
            }
            var resultJson = await HttpHelper.GetAsync(requestUri, AccessToken);
            var result = JsonConvert.DeserializeObject<ListGroupsObj>(resultJson);
            return result;
        }
        public async Task<Byte[]> GetUserPhotoValue(string userId)
        {
            var requestUri = $"{GraphEndPoint}/{ApiVersion}/users/{userId}/photo/$value";

            var result = await HttpHelper.GetByteAsync(requestUri, AccessToken);
            return result;
        }

        public async Task<GraphUser> FilterGraphUserByEmail(string mail)
        {
            var requestUri = $"{GraphEndPoint}/{ApiVersion}/users";
            var resultJson = await HttpHelper.GetAsync(requestUri, AccessToken);
            var users = JsonConvert.DeserializeObject<ListUsersObj>(resultJson);
            var temp = users.Value.ToList();
            GraphUser result = null;
            foreach (var re in temp)
            {
                if (re.Mail != null && re.Mail.Equals(mail, StringComparison.OrdinalIgnoreCase))
                {
                    result=re; 
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// check if a user is a transitive member of the specified groups.
        /// Limitation: Up to 20 group IDs per request.
        /// </summary>
        /// <param name="userUpn">The user principal name or object ID of the user to check.</param>
        /// <param name="groupIds">A list of group IDs to check membership against (max 20).</param>
        /// <returns>A list of group IDs from the input that the user is a member of.</returns>
        public async Task<List<string>> CheckUserMemberGroups(string userUpn, List<string> groupIds)
        {
            var encodedUpn = Uri.EscapeDataString(userUpn);
            var requestUri = $"{GraphEndPoint}/{ApiVersion}/users/{encodedUpn}/checkMemberGroups";
            var requestBody = JsonConvert.SerializeObject(new { groupIds });
            var resultJson = await HttpHelper.PostAsync(requestUri, requestBody, AccessToken);
            var result = JsonConvert.DeserializeObject<CheckMemberGroupsResponse>(resultJson);
            return result?.Value ?? new List<string>();
        }
    }
}
