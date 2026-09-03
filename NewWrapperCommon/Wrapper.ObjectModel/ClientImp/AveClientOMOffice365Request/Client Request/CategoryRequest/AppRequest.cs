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
using AveClientRequest.Common;
using AvePoint.Wrapper.Common;
using Microsoft.Online.SharePoint.TenantAdministration;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.ObjectModel.ClientOM
{
    public partial class AveClientOMOffice365Request: AveClientOM2019Request
    {
        [ReplaceByAPI]
        public override Dictionary<string, object> GetWebAppById(string webServerRelativeUrl, Guid appId)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                AppInstance apps = web.GetAppInstanceById(appId);
                context.Load(apps);
                context.ExecuteQuery();
                Dictionary<string, object> appInstanceProperties = new Dictionary<string, object>();
                CopyProperty(appInstanceProperties, apps);
                if (!string.IsNullOrEmpty(apps.AppWebFullUrl))
                {
                    appInstanceProperties["AppWebFullUrl"] = new Uri(apps.AppWebFullUrl);
                }
                Dictionary<string, object> appProperties = new Dictionary<string, object>();
                appProperties["ProductId"] = apps.ProductId;
                appProperties["Source"] = GetAppSource(appId, context, web);
                appInstanceProperties["App"] = appProperties;
                return appInstanceProperties;
            }
        }
        private AveAppSource GetAppSource(Guid appId, AveClientContext context, Web web)
        {
            var tiles = web.AppTiles;
            context.Load(tiles, tileCollection => tileCollection.Where(tile => tile.AppId == appId).Include(tile => tile.AppSource));
            context.ExecuteQuery();
            if (tiles != null && tiles.Count > 0) return (AveAppSource)(int)(tiles[0].AppSource);
            return AveAppSource.InvalidSource;
        }

        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetApps(string webServerRelativeUrl)
        {
            return base.GetApps(webServerRelativeUrl);
        }

        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetAppsByProductId(string webServerRelativeUrl, Guid productId)
        {
            return base.GetAppsByProductId(webServerRelativeUrl, productId);
        }

        [NoAPI("No API to trust an app.")]
        public override Dictionary<string, object> RestoreApp(string webServerRelativeUrl, AveAppPackageInfo appInfo, Dictionary<string, object> restoreInfo)
        {
            var token = tokenProviders.GetProviderByType(Office365.Api.TokenType.IDCLR);
            if (token == null)
            {
                mLogger.Warn("App token does not support restore App.");
                Dictionary<string, object> appsProperties = new Dictionary<string, object>();
                List<Dictionary<string, object>> appPropertyList = new List<Dictionary<string, object>>();
                appsProperties[AveObjectModelConstant.ChildrenProperties] = appPropertyList;
                return appsProperties;
            }
            using (AveClientContext context = CreateContext())
            {
                Site site = context.Site;
                Web web = site.OpenWeb(webServerRelativeUrl);
                string webFullUrl = this.WebAppName + webServerRelativeUrl;
                AveAppRestore appRestore = new Ave365AppRestore(context, token, site, web, webFullUrl);
                appRestore.RestoreApp(appInfo, restoreInfo);
                return GetAppsByProductId(webServerRelativeUrl, appInfo.ProductId);
            }
        }
        [KeepOriginalWithAPI]
        public override Guid UninstallAppByInstanceId(Guid webId, Guid instanceId, Guid productId, bool waitUninstallFinish)
        {
            return base.UninstallAppByInstanceId(webId, instanceId,productId, waitUninstallFinish);
        }
    }
}
