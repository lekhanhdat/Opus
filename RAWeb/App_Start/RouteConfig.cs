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
using Microsoft.AspNetCore.Builder;

namespace AvePoint.RA.Web.Config
{
    public static class RouteConfig
    {
        public static void RegisterRoutes(this WebApplication app)
        {
            //app.MapControllerRoute(
            //    name: "default",
            //    pattern: "{controller=Home}/{action=Index}/{id?}");

#if DEBUG
            
            //routes.IgnoreRoute("{*browserlink}", new { browserlink = @".*/arterySignalR/ping" });
#endif

            app.MapControllerRoute(
                name: "sso",
                pattern: "sso",
                defaults: new { controller = "Account", action = "SSOLogin" });

            app.MapControllerRoute(
                name: "healthz",
                pattern: "healthz",
                defaults: new { controller = "Healthz", action = "Get" }
            );

			app.MapControllerRoute(
				name: "Related",
				pattern: "RelatedRecords",
				defaults: new { controller = "RelatedRecords", action = "Index" }
			);

			app.MapControllerRoute(
                name: "Default",
                pattern: "{controller}/{action}",
                defaults: new { controller = "Account", action = "SSOLogin" }
            );

            app.MapControllerRoute("JsResource", "RMWeb/JsResx");

            app.MapControllerRoute(
                name: "Account",
                pattern: "{controller}/{action}",
                defaults: new { controller = "Account", action = "LogOn" }
            );
            
            app.MapControllerRoute(
                name: "SPA",
                pattern: "Root/{moudle?}/{subMoudle?}/{page?}",
                defaults: new { controller = "Root", action = "Home" }
            );

        }
    }
}
