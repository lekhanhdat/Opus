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
using AngleSharp.Common;
using AvePoint.GCommon;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.SharePoint.Common;
using AvePoint.Wrapper.Common;
using Cloud.Sdk.AosModern;
using Cloud.Sdk.Data.AosModern;
using Cloud.Sdk.Token;
using DocAveOnline.WebApi.Contracts;
using Microsoft.Azure.StackExchangeRedis;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Search.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace AvePoint.RA.Api.Services.Search
{
    public class StubSearch
    {
        private Dictionary<string,List<string>> FileUrlMapping = new Dictionary<string, List<string>>();
        private static AveLogger logger = AveLogger.GetInstance(typeof(StubSearch));
        private ClientContext SearchContext;
        private Microsoft365User mMicrosoft365User;
        private List<string> SearchResultUrls = new List<string>();
        private AppProfileInfo appProfile;
        private string UserLoginName;
        public StubSearch(Microsoft365User microsoft365User)
        {
            mMicrosoft365User = microsoft365User;
            appProfile = PoolUserUtil.GetBPOSInfoAsync(mMicrosoft365User.TenantId).GetAwaiter().GetResult();
            if (appProfile == null)
            {
                logger.Warn($"app profile is null when get opus app,need to get aosp app,tenant id:{TenantLocalValue.LogonGroupId}");
                appProfile = RMAosApiClient.GetAOSPAuthProfile(TenantLocalValue.LogonGroupId, mMicrosoft365User.TenantId).GetAwaiter().GetResult();
                logger.Warn($"aosp profile name:{appProfile?.Name},id:{appProfile?.Id}");
                if (string.IsNullOrEmpty(appProfile?.TenantId))
                {
                    logger.Warn($"StubSearch current aops app not have 365tenant id,need set it.365tenant id:{microsoft365User.TenantId}");
                    appProfile.TenantId = microsoft365User.TenantId;
                }
            }
        }
        public async Task<List<string>> GetHasPermissionStubUrls() 
        {
            await GetSearchResult();
            await CheckPermission();
            logger.Info($"this user has permission stubs count is:{SearchResultUrls.Count}");
            return SearchResultUrls;
        }

        private async Task GetSearchResult()
        {
            logger.Info("start to get search result");
            var result = await GetToken(appProfile.AdminUrl);
            string token = result.AccessToken;
            string accessToken = token;
            using (SearchContext = new ClientContext(appProfile.AdminUrl))
            {
                SearchContext.ExecutingWebRequest +=
            (sender, e) => e.WebRequestExecutor.WebRequest.Headers["Authorization"] = "Bearer " + accessToken;
                var user = SearchContext.Web.EnsureUser(mMicrosoft365User.UserEmail);
                SearchContext.Load(user);
                SearchContext.ExecuteQuery();
                UserLoginName = user.LoginName;
                KeywordQuery keywordQuery = new KeywordQuery(SearchContext);
                keywordQuery.SelectProperties.Add("SiteName");
                keywordQuery.SelectProperties.Add("Path");
                keywordQuery.TrimDuplicates = false;
                keywordQuery.RowLimit = 10;
                keywordQuery.StartRow = 0;
                keywordQuery.EnableSorting = true;
                keywordQuery.Culture = 1033;
                keywordQuery.QueryText = mMicrosoft365User.StubId;
                SearchExecutor searchExecutor = new SearchExecutor(SearchContext);
                var results = searchExecutor.ExecuteQuery(keywordQuery);
                SearchContext.ExecuteQuery();
                var realResult = results.Value[0].ResultRows;
                logger.Info($"search result count:{realResult?.Count()}");
                InitAllSiteUrls(realResult);
                logger.Info($"finish to get search result");
            }
        }
        private async Task<Cloud.Sdk.Data.AosModern.TokenResult> GetToken(string Url)
        {
            if (TenantLocalValue.CallerType == "PartnerPortal" || appProfile.Type == IdentityProviderType.AospSecurityAnalysis || appProfile.Type == IdentityProviderType.AospSecurityAnalysisCsp)
            {
                var tokenApiClient = AosApiUtility.CloudSdkTokenClientFactory.CreateModernTokenApiClient(TenantLocalValue.LogonGroupId);
                var tokenResult = tokenApiClient.ImpersonateCallerInvoke<ModernTokenApiClient, Cloud.Sdk.Data.AosModern.TokenResult?>(Cloud.Sdk.Data.Core.CallerType.PartnerPortal, async (client) =>
                {
                    var result = await client.ModernTokenService.GetTokenByAppProfileAsync(
                        appProfile.Type,
                        TokenResourceType.SharePoint,
                        appProfile.TenantId,
                        appProfile.Id,
                        new Uri(Url).GetLeftPart(UriPartial.Authority),
                        Cloud.Sdk.Data.AosModern.TokenType.ApplicationToken
                    );
                    return result;
                }).GetAwaiter().GetResult();
                return tokenResult;
            }
            else
            {
                return await AosApiUtility.CloudSdkTokenClientFactory.CreateModernTokenApiClient(TenantLocalValue.LogonGroupId).ModernTokenService.GetTokenByAppProfileAsync(
                    appProfile.Type,
                    TokenResourceType.SharePoint,
                    appProfile.TenantId,
                    appProfile.Id,
                    new Uri(Url).GetLeftPart(UriPartial.Authority),
                    Cloud.Sdk.Data.AosModern.TokenType.ApplicationToken
                );
            }
        }
        private async Task CheckPermission()
        {
            foreach (var map in FileUrlMapping)
            {
                logger.Info($"start check stub permission,current stub site:{map.Key}");
                var result = await GetToken(map.Key);
                using (var context = new ClientContext(map.Key))
                {
                    context.ExecutingWebRequest +=
                (sender, e) => e.WebRequestExecutor.WebRequest.Headers["Authorization"] = "Bearer " + result.AccessToken;
                    RetrieveFiles(context, map.Value,UserLoginName);
                }
            }
        }
        private void InitAllSiteUrls(IEnumerable<IDictionary<string, object>> searchResult)
        {
            foreach (var item in searchResult) 
            {
                string siteUrl = item["SiteName"].ToString();
                string fileId = item["Path"].ToString();
                if (!FileUrlMapping.ContainsKey(siteUrl))
                {
                    FileUrlMapping.Add(siteUrl, new List<string> { fileId });
                }
                else
                {
                    FileUrlMapping[siteUrl].Add(fileId);
                }
            }
        }
        private void RetrieveFiles(ClientContext context, List<string> _fileIds, string userLoginName)
        {

            // Retrieve files from root web
            RetrieveFilesFromWeb(context.Web, _fileIds, userLoginName);

            // Retrieve files from sub webs
            RetrieveFilesFromSubWebs(context.Web, _fileIds, userLoginName);

        }

        private void RetrieveFilesFromWeb(Microsoft.SharePoint.Client.Web web, List<string> fileUrls,string userLoginName)
        {
            foreach (var url in fileUrls)
            {
                try
                {
                    var serverRaletedurl = AveUrlUtility.GetServerRelativeUrl(url);
                    File file = web.GetFileByServerRelativePath(ResourcePath.FromDecodedUrl(serverRaletedurl));
                    web.Context.Load(file);
                    web.Context.ExecuteQuery();
                    var pRes = file.ListItemAllFields.GetUserEffectivePermissions(userLoginName);
                    web.Context.ExecuteQuery();
                    if (pRes != null && pRes.Value.Has(PermissionKind.OpenItems | PermissionKind.ViewPages | PermissionKind.ViewListItems | PermissionKind.ViewPages))
                    {
                        string fileName = Uri.EscapeDataString(url.Substring(url.LastIndexOf("/")+1));
                        string containerName = url.Substring(0,url.LastIndexOf("/"));
                        SearchResultUrls.Add(containerName+"/"+ fileName);
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn($"Failed to retrieve file with url {url}: {ex.Message}");
                }
            }
        }

        private void RetrieveFilesFromSubWebs(Microsoft.SharePoint.Client.Web web, List<string> fileUrls, string userLoginName)
        {
            web.Context.Load(web.Webs);
            web.Context.ExecuteQuery();

            foreach (Microsoft.SharePoint.Client.Web subWeb in web.Webs)
            {
                RetrieveFilesFromWeb(subWeb, fileUrls, userLoginName);
                RetrieveFilesFromSubWebs(subWeb, fileUrls, userLoginName); // Recursively retrieve files from nested sub webs
            }
        }
    }
}
