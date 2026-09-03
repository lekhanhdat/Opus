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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.Cache;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Web.Extentions.Util;
using Azure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.SharePoint.Client;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace RASPAppWeb.Middleware
{
    public class RelatedRecordsHandlerMiddleware
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(RelatedRecordsHandlerMiddleware));

        private RequestDelegate _next;
        private string relatedId;//need set in SharePoint setting , value is Tanant ID in AOS.
        private string spHostUrl;
        private string o365TenantId;
        private string o365Domain;

        public RelatedRecordsHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            await ProcessRequest(context);
        }


        private void InitParams(HttpContext context)
        {
            spHostUrl = context.Request.Query[SharePointContext.SPHostUrlKey];
            //senderId = context.Request.Query[SharePointContext.SenderId];
            var spHostUri = new Uri(spHostUrl);
            o365Domain = spHostUri.Authority.Substring(0, spHostUri.Authority.IndexOf('.'));
            var spLanguage = context.Request.Query[SharePointContext.SPLanguageKey];
            I18nUtil.SetLanguage(spLanguage);
        }

        private async Task ProcessRequest(HttpContext context)
        {
            //context.Response.ContentType = "text/plain";
            //context.Response.Write("Hello World");
            logger.Info("Begin process request .....");
            logger.Info("Validate request url");
            string originalHost = context.Request.Host.Host;
            logger.Info($"Host url {originalHost}");
            if (context.Request.Headers.Keys.Any(a => a.Equals("X-Original-Host", StringComparison.OrdinalIgnoreCase)))
            {
                string originalHostKey = context.Request.Headers.Keys.FirstOrDefault(a => a.Equals("X-Original-Host", StringComparison.OrdinalIgnoreCase));
                originalHost = context.Request.Headers.GetHeaderValue(originalHostKey);
                logger.Info("Original Host {0}, URL: {1}", originalHostKey, originalHost);
            }
            try
            {
                InitParams(context);
                if (!ValidateSPToken(context, originalHost))
                {
                    logger.Warn("Validate token failed ");
                    var tokenError = RASPAppWeb.Resources.RelatedRecords.RM_App_TokenError;
                    #region DEBUG
                    //try
                    //{
                    //    relatedId = "5c8375d4-db90-4ce6-9f42-c0b47eef0e4c";
                    //    if (TenantService.ValidateTenant(o365TenantId, o365Domain, relatedId))
                    //    {
                    //        logger.Info("$$$fordebug success");
                    //    }
                    //}
                    //catch (Exception e)
                    //{
                    //    logger.Info($"$$$failed {e}");
                    //}
                    #endregion
                    await context.Response.WriteAsync(tokenError);
                }
                else
                {
                    relatedId = GetAveId(spHostUrl, context, originalHost);
                    logger.Info("get related id successfully {0}", relatedId);
                    //aveId = "5c8375d4-db90-4ce6-9f42-c0b47eef0e4c";//for debug 
                    if (TenantService.ValidateTenant(o365TenantId, o365Domain, relatedId))
                    {
                        
                        var url = GetServiceUrl(context, originalHost);

                        var domain = originalHost[(originalHost.IndexOf(".") + 1)..];
                        var contextTokenString = TokenHelper.GetContextTokenFromRequest(context.Request);
                        context.Response.Cookies.Delete(SPAppConstants.ParamRelateToken);
                        context.Response.Cookies.Append(SPAppConstants.ParamRelateToken, contextTokenString, new CookieOptions { 
                            Domain = domain,
                            Secure = true,
                            HttpOnly = true,
                            SameSite = SameSiteMode.Lax, 
                        });


                        context.Response.Redirect(url, false);
                    }
                    else
                    {
                        logger.Warn("Validate tenant failed {0}", relatedId);
                        var tanentError = RASPAppWeb.Resources.RelatedRecords.RM_App_TenantError;

                        //for debug
                        throw new Exception(tanentError);
                    }

                }
            }
            catch (Exception e)
            {
                logger.Error("Error occurred while processing request, {0}", e);
                await context.Response.WriteAsync(e.Message);
            }
        }
        private bool ValidateSPToken(HttpContext context, string host)
        {
            var isValid = false;
            string contextTokenString = TokenHelper.GetContextTokenFromRequest(context.Request);
            logger.Info("get context token success");
            if (!string.IsNullOrEmpty(contextTokenString))
            {
                try
                {
                    var token = TokenHelper.ReadAndValidateContextToken(contextTokenString, host);
                    logger.Info("Domain:" + o365Domain);
                    if (o365Domain.Contains("-my"))
                    {
                        o365Domain = o365Domain.Replace("-my", "");
                        logger.Info(o365Domain);
                    }
                    o365TenantId = O365Util.GetO365TenantId(o365Domain);
                    logger.Info("356Id" + o365TenantId);
                    if (token.Realm == o365TenantId)
                    {
                        isValid = true;
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn("Error occurred when validate sp token {0}", ex.ToString());
                    isValid = false;
                }
            }
            return isValid;
        }

        public string GetAveId(string siteUrl, HttpContext context, string host)
        {
            Func<string> getObj = () =>
            {
                string contextTokenString = TokenHelper.GetContextTokenFromRequest(context.Request);
                SharePointContextToken contextToken = TokenHelper.ReadAndValidateContextToken(contextTokenString, host);
                var spHostUri = new Uri(spHostUrl);
                string accessToken = TokenHelper.GetAccessToken(contextToken, spHostUri.Authority).AccessToken;
                using (ClientContext clientContext = TokenHelper.GetClientContextWithAccessToken(siteUrl, accessToken))
                {
                    var p = clientContext.Web.AllProperties;
                    clientContext.Load(p);
                    clientContext.ExecuteQuery();
                    if (p.FieldValues.ContainsKey("RelatedId"))
                    {
                        logger.Info("get related id from {0}", o365Domain);
                        return p["RelatedId"]?.ToString();
                    }
                    //return "5c8375d4-db90-4ce6-9f42-c0b47eef0e4c";//for debug
                    // return null;
                    logger.Warn($"get related id failed {siteUrl},{o365Domain}");
                    throw new Exception(I18NEntity.GetString("RM_RD_Delete_ReatedIdApplyError"));
                }
            };
            return CacheService.Get("RelatedId", o365Domain, getObj, TimeSpan.FromMinutes(30));
        }
        public string GetServiceUrl(HttpContext context, string host)
        {
            var spLanguage = context.Request.Query[SharePointContext.SPLanguageKey];
            var aveWebId = TenantUtil.GetAveId(spHostUrl);
            var serviceUrl = RMAosApiClient.GetRecordsServiceUrl(relatedId);//debug (aveId);
            var itemId = context.Request.Query["SPListItemId"];
            var listId = context.Request.Query["SPListId"];
            //var siteUrl = context.Request.Query["su"];
            var queryNameValueCollection = HttpUtility.ParseQueryString(string.Empty);
            queryNameValueCollection.Add(SPAppConstants.ParamHostUrl, spHostUrl);
            //queryNameValueCollection.Add(SPAppConstants.ParamAccessToken, TokenHelper.GetContextTokenFromRequest(context.Request));
            queryNameValueCollection.Add(SPAppConstants.ParamLanguage, spLanguage);
            queryNameValueCollection.Add(SPAppConstants.ParamDomain, o365Domain);
            queryNameValueCollection.Add(SPAppConstants.ParamListId, listId);
            queryNameValueCollection.Add(SPAppConstants.ParamItemId, itemId);
            queryNameValueCollection.Add(SPAppConstants.ParamTenantId, relatedId);//not use now
            queryNameValueCollection.Add(SPAppConstants.ParamAppHost, host);
            queryNameValueCollection.Add(SPAppConstants.ParamRelateRedirectSign, "1");
            UriBuilder returnUrlBuilder = new UriBuilder($"{serviceUrl}/RelatedRecords");
            returnUrlBuilder.Query = queryNameValueCollection.ToString();

            return returnUrlBuilder.Uri.AbsoluteUri;
        }
    }


    public static class RelatedRecordsMiddlewareExtensions
    {
        public static IApplicationBuilder UseRelatedRecordsHandlerMiddleware(this IApplicationBuilder applicationBuilder)
        {
            return applicationBuilder.UseMiddleware<RelatedRecordsHandlerMiddleware>();
        }
    }
}
