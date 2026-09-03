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
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.Cache;
using AvePoint.RA.Common.Graph;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.GraphApi;
using AvePoint.RA.Contract.Object;
using Cloud.Sdk.Data.AosModern;
using Microsoft.Graph;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AvePoint.GCommon.Utility.I18N.ContextValues.Configuration;

namespace AvePoint.RA.Common.GraphApi.App
{
    public class RMGraphAppManager : RMGraphApiManager
    {
        public RMGraphAppManager(AppProfileInfo profile) : base(profile)
        {}

        public async Task<bool> HasApiPermission(string resourceAppId, string resourceAccessId)
        {
            var requestUri = $"{GraphEndPoint}/{ApiVersion}/applications(appId='{Profile.AppClientId}')/requiredResourceAccess";

            var resultJson = await HttpHelper.GetAsync(requestUri, AccessToken);
            var result = JsonConvert.DeserializeObject<RMGraphApiResponse<List<RMAADAppApiPermissionInfo>>>(resultJson);

            var permissionInfoes = result.Value;
            var specifyPermissionApp = permissionInfoes.FirstOrDefault(item => item.ResourceAppId.Equals(resourceAppId, StringComparison.OrdinalIgnoreCase));

            if(specifyPermissionApp == null)
            {
                return false;
            }

            return specifyPermissionApp.ResourceAccessList.Any(item => item.Id.Equals(resourceAccessId, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<bool> HasApiPermission(Dictionary<string, List<string>> permissions)
        {
            var requestUri = $"{GraphEndPoint}/{ApiVersion}/applications(appId='{Profile.AppClientId}')/requiredResourceAccess";

            var resultJson = await HttpHelper.GetAsync(requestUri, AccessToken);
            var result = JsonConvert.DeserializeObject<RMGraphApiResponse<List<RMAADAppApiPermissionInfo>>>(resultJson);

            var permissionInfoes = result.Value;
            foreach(var entity in permissions)
            {
                var specifyPermissionApp = permissionInfoes.FirstOrDefault(item => item.ResourceAppId.Equals(entity.Key, StringComparison.OrdinalIgnoreCase));

                if (specifyPermissionApp == null)
                {
                    return false;
                }

                var matchedAll = entity.Value.All(item => specifyPermissionApp.ResourceAccessList.Any(item2 => item2.Id.Equals(item, StringComparison.OrdinalIgnoreCase)));
                if(!matchedAll)
                {
                    return false;
                }
            }

            return true;
        }

        public bool HasSendEmailPermission()
        {
            var needPermissions = new HashSet<string>
            {
                "mail.send",
            };
            var token = new JwtSecurityTokenHandler().ReadJwtToken(AccessToken);
            var tokenPermissions = token.Claims.Where(c => c.Type == "roles").Select(r => r.Value.ToLower()).ToHashSet();
            return needPermissions.All(item => tokenPermissions.Contains(item));
        }
    }
}
