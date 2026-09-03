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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility;
using AvePoint.RA.Web.Common.Context;
using AvePoint.RA.Web.Common.WIF;
using AvePoint.RA.Web.Extentions.Authorize;
using AvePoint.RA.Web.Extentions.SharePoint;
using AvePoint.RA.Web.Extentions.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace AvePoint.RA.Web.Controllers.RelatedRecords
{
    [AllowAnonymous]
    [RMAppAuthorize]
    public class RelatedRecordsController : Controller
    {
        private RALogger logger = RALogger.GetInstance(typeof(RelatedRecordsController));
        public IGeneralSettingService GeneralSettingService
        {
            get
            {
                return (IGeneralSettingService)PlatformWindsorManager.GetService(typeof(IGeneralSettingService));
            }
        }

        private String[] injectionScripts = new String[4];

        public const String HeadScriptAppendContent = "HeadScriptAppendContent";
        public const String BodyScriptAppendContent = "BodyScriptAppendContent";
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            ProcessAllInjectScripts();
        }
        private void SetCulture()
        {

            string cultureName = null;

            var language = Request.Query[SPAppConstants.ParamLanguage];
            if (!string.IsNullOrEmpty(language))
            {
                cultureName = language;
            }
            else
            {
                // obtain it from HTTP header AcceptLanguages
                var languages = Request.GetTypedHeaders().AcceptLanguage;
                if (languages != null && languages.Count > 0)
                {
                    cultureName = languages.First().Value.Value;
                }
            }
            System.Globalization.CultureInfo ci = null;
            try
            {
                ci = System.Globalization.CultureInfo.CreateSpecificCulture(cultureName);
            }
            catch
            {
                ci = EnvironmentContext.GetDefaultCulture();
            }

            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;
        }

        // GET: RelatedRecords
        //[AllowAnonymous]
        //[RMAuthorizeFilterAttribute(RequireAuthentication = false)]
        public async Task<ActionResult> Index()
        {
            using (new PerformanceScope("RelatedRecordsController--Index"))
            {
                try
                {
                    SetCulture();
                    ViewData["defaultlocation"] = Request.Query[SPAppConstants.ParamHostUrl];
                    //load data init context....
                    var hostUrl = Request.Query[SPAppConstants.ParamHostUrl];
                    string accessToken = string.Empty;
                    string contextTokenString = string.Empty;
                    if (Request.Cookies.ContainsKey(SPAppConstants.ParamRelateToken))
                    {
                        logger.Info($"Init request success {hostUrl} ");

                        using (new PerformanceScope("RelatedRecordsController--GetAccessToken"))
                        {
                            accessToken = ReletedRecordsAppTokenHelper.GetAccessToken(
                                Request.Cookies[SPAppConstants.ParamRelateToken],
                                Request.Query[SPAppConstants.ParamAppHost],
                                Request.Query[SPAppConstants.ParamHostUrl]
                            ); 
                        }
                        var listId = Request.Query[SPAppConstants.ParamListId];
                        var itemId = Request.Query[SPAppConstants.ParamItemId];
                        using (new PerformanceScope("RelatedRecordsController--RelatedRecordsUtility"))
                        {
                            using (var utility = new RelatedRecordsUtility(hostUrl, accessToken, new Guid(listId), Convert.ToInt32(itemId)))
                            {
                                //utility.AddRelatedColumn();
                                //utility.AddRelatedColumnTolist();
                                using (new PerformanceScope("RelatedRecordsController--RelatedRecordsUtility--InitSetting"))
                                {
                                    await InitSettingAsync(utility);
                                }
                                var relatedInfos = utility.GetRelatedProperties();
                                if (relatedInfos != null)
                                {
                                    ViewData["RelatedInfos"] = JsonConvert.SerializeObject(relatedInfos);
                                }
                                var folderUrl = utility.folderUrl;
                                if (!string.IsNullOrEmpty(folderUrl))
                                {
                                    ViewData["NavigateUrl"] = folderUrl;
                                }
                                var currItemName = utility.GetCurrentItemName();
                                if (!string.IsNullOrEmpty(currItemName))
                                {
                                    ViewData["CurrentItemName"] = currItemName;
                                }
                            }
                        }
                    }
                    else
                    {
                        //Gov App need validate token here
                        string originalHost = string.Empty;
                        try
                        {
                            if (Request.Headers.Keys.Any(a => a.Equals("X-Original-Host", StringComparison.OrdinalIgnoreCase)))
                            {
                                string originalHostKey = Request.Headers.Keys.FirstOrDefault(a => a.Equals("X-Original-Host", StringComparison.OrdinalIgnoreCase));
                                originalHost = Request.Headers.GetHeaderValue(originalHostKey);
                                logger.Info("Original Host {0}, URL: {1}", originalHostKey, originalHost);
                            }
                            contextTokenString = Request.GetContextTokenFromRequest();
                            logger.Info("Get context token success");
                        }
                        catch (Exception ex)
                        {
                            logger.Info($"Get context token failed {ex.ToString()}");
                        }
                        Response.Cookies.Delete(SPAppConstants.ParamRelateToken);
                        Response.Cookies.Append(SPAppConstants.ParamRelateToken, contextTokenString, new CookieOptions
                        {
                            Secure = true,
                            HttpOnly = true,
                            SameSite = SameSiteMode.Lax,
                            Domain = HttpContext.Request.Host.Host,
                        });


                        Response.Redirect(GetServiceUrl(Request, contextTokenString), false);
                    }
                }
                catch (Exception e)
                {
                    logger.Warn($"Init Related page failed {e.ToString()}");
                    throw;
                }
            }

            var nonce = HttpContext.Items["Nonce"] as string;
            ViewBag.Nonce = nonce;
            return View();
        }
        public string GetServiceUrl(HttpRequest Request, string accessToken)
        {
            var spLanguage = Request.Query["SPLanguage"];
            #region get access token
            string originalHost = string.Empty;
            string originalUrl = string.Empty;
            if (Request.Headers.Keys.Any(a => a.Equals("X-Original-Host", StringComparison.OrdinalIgnoreCase)))
            {
                string originalHostKey = Request.Headers.Keys.FirstOrDefault(a => a.Equals("X-Original-Host", StringComparison.OrdinalIgnoreCase));
                originalHost = Request.Headers.GetHeaderValue(originalHostKey);
                logger.Info("Original Host {0}, URL: {1}", originalHostKey, originalHost);
            }

            var spHostUrl = Request.Query[SPAppConstants.ParamHostUrl];
            var aveWebId = TenantUtil.GetAveId(spHostUrl);
            if (Request.Headers.Keys.Any(a => a.Equals("X-Original-URL", StringComparison.OrdinalIgnoreCase)))
            {
                string originalHostKey = Request.Headers.Keys.FirstOrDefault(a => a.Equals("X-Original-URL", StringComparison.OrdinalIgnoreCase));
                originalUrl = Request.Headers.GetHeaderValue(originalHostKey);
                logger.Info("Original URL {0}, URL: {1}", originalHostKey, originalUrl);
            }
            Uri serviceUrl = null;
            if (!string.IsNullOrEmpty(originalUrl))
            {
                string serviceURI = string.Empty;
                if (!originalUrl.StartsWith("https"))
                {
                    serviceURI = "https://" + originalHost + originalUrl;
                }
                else
                {
                    serviceURI = originalUrl;
                }
                logger.Info($"Service uri {serviceURI}");
                serviceUrl = new Uri(serviceURI);
            }
            else
            {
                serviceUrl = Request.GetUrl();
            }
            #endregion
            var itemId = Request.Query["SPListItemId"];
            var listId = Request.Query["SPListId"];
            //var siteUrl = context.Request.Query["su"];
            var queryNameValueCollection = HttpUtility.ParseQueryString(string.Empty);
            queryNameValueCollection.Add(SPAppConstants.ParamHostUrl, spHostUrl);
            //queryNameValueCollection.Add(SPAppConstants.ParamAccessToken, accessToken);
            queryNameValueCollection.Add(SPAppConstants.ParamLanguage, spLanguage);

            queryNameValueCollection.Add(SPAppConstants.ParamListId, listId);
            queryNameValueCollection.Add(SPAppConstants.ParamItemId, itemId);
            queryNameValueCollection.Add(SPAppConstants.ParamAppHost, originalHost);
            queryNameValueCollection.Add(SPAppConstants.ParamRelateRedirectSign, "1");
            UriBuilder returnUrlBuilder = new UriBuilder($"{serviceUrl}");
            returnUrlBuilder.Query = queryNameValueCollection.ToString();

            return returnUrlBuilder.Uri.AbsoluteUri;
        }

        private async Task InitSettingAsync(RelatedRecordsUtility utility)
        {
            try
            {
                var redirectUrl = string.Empty;
                
                redirectUrl = Request.Cookies.Keys.Contains(AuthCookie.CookieName) ? WebUtil.GetRecordsHomePageUrl() : WebUtil.GetRedirectRecodsSSOLoginUrl();
                ViewData["RedirectHomeUrl"] = redirectUrl;

                var tenantId = Request.Query[SPAppConstants.ParamTenantId];
                if (string.IsNullOrEmpty(tenantId))
                {
                    logger.Info($"try to get tenantId by web prop");
                    tenantId = utility.GetTenantId();
                }
                ThrowUtil.ThrowIfNullOrEmpty(tenantId, "tenant Id empty.");
                TenantLocalValue.LogonGroupId = tenantId;
                TimeSettingModel tsm = await GeneralSettingService.GetTimeSettingModelAsync(tenantId);
                var scripts = string.Format("var RM=RM||{{}};RM.TimeSettingModel={0};", JsonConvert.SerializeObject(tsm));
                AppendScriptToHead(scripts);
            }
            catch (Exception ex)
            {
                logger.Error("init setting error:{0}", ex.ToString());
            }

        }

        protected void AppendScriptToHead(String script)
        {
            injectionScripts[0] += Environment.NewLine + script;
        }
        private void ProcessAllInjectScripts()
        {
            ProcessInjectionScript(HeadScriptAppendContent, injectionScripts[0]);
            ProcessInjectionScript(BodyScriptAppendContent, injectionScripts[1]);
        }
        private void ProcessInjectionScript(String key, String script)
        {
            if (!String.IsNullOrEmpty(script))
            {
                ViewData[key] = script;
            }
        }
        // GET: RMWeb
        /// <summary>
        /// 此Controller 返回资源文件 缓存360天
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public string JsResx()
        {
            var script = RMResourceManager.GetResourceScript();
            Response.WriteAsync(script).GetAwaiter().GetResult();
            return script;
        }

    }
}