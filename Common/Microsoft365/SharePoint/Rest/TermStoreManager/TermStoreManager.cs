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

using Microsoft365.Authentication.TokenProvider;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Microsoft365.SharePoint.Rest
{
    public class TermStoreManager
    {
        private const string apiBase = "/_api/v2.1/termstore";
        private const string JSON_ContentType = "application/json";
        private const string DefaultQuery = "?$SELECT=*,administrators,oneDrive.rights";
        private const string HTTPMETHOD_PATCH = "Patch";
        private const string SharePoint_App_UserName = "i:0i.t|00000003-0000-0ff1-ce00-000000000000|app@sharepoint";
        protected string AdminSiteUrl { get; set; }
        private SharePointRestExecutor SharePointRestExecutor { get; set; }
        public TermStoreManager(string adminSiteUrl, IATokenProvider tokenProvider)
        {
            AdminSiteUrl = adminSiteUrl;
            SharePointRestExecutor = new SharePointRestExecutor(AdminSiteUrl, tokenProvider, true);
        }

        public ListTermStoreAdminUsersResult GetTermStoreAdminUsers()
        {
            var requestUrl = $"{AdminSiteUrl.TrimEnd('/')}{apiBase}{DefaultQuery}";
            return SharePointRestExecutor.Get<ListTermStoreAdminUsersResult>(new Uri(requestUrl), null);
        }
        public ListTermStoreAdminUsersResult AddSharePointAppToTermStoreAdmin()
        {
           return AddTermStoreAdmin(SharePoint_App_UserName);
        }
        public ListTermStoreAdminUsersResult AddTermStoreAdmin(string userLogin)
        {
            var contentObj = GetTermStoreAdminUsers();
            var requestUrl = $"{AdminSiteUrl.TrimEnd('/')}{apiBase}{DefaultQuery}";
            contentObj.administrators.Add(new TermStoreAdminUser
            {
                user = new User
                {
                    userPrincipalName = userLogin
                }
            });
            string contentStr = JsonConvert.SerializeObject(contentObj, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            StringContent content = AddJsonContent(contentStr);

            return SharePointRestExecutor.Execute<ListTermStoreAdminUsersResult>(
                new Uri(requestUrl),
                new HttpMethod(HTTPMETHOD_PATCH),
                content);
        }

        private static StringContent AddJsonContent(string contentStr)
        {
            var content = new StringContent(contentStr);
            content.Headers.ContentType = new MediaTypeHeaderValue(JSON_ContentType);
            return content;
        }
    }
}