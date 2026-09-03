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
using AvePoint.RA.I18N.Core;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;

namespace AvePoint.RA.Web.Extentions.Util
{
    public static class I18NEntityExtensionOnHtmlHelper
    {

        public static HtmlString I18N(this IHtmlHelper helper, ModuleName moduleName, string key)
        {
            return new HtmlString(I18NEntity.GetString(moduleName, key));
        }

        public static HtmlString I18N(this IHtmlHelper helper, ModuleName moduleName, string key, CultureInfo culture)
        {
            return new HtmlString(I18NEntity.GetString(moduleName, key, culture));
        }

        public static HtmlString I18N(this IHtmlHelper helper, ModuleName moduleName, string key, params object[] args)
        {
            return new HtmlString(I18NEntity.GetString(moduleName, key, args));
        }

        public static HtmlString I18N(this IHtmlHelper helper, ModuleName moduleName, string key, CultureInfo culture, params object[] args)
        {
            return new HtmlString(I18NEntity.GetString(moduleName, key, culture, args));
        }
        public static HtmlString I18N(this IHtmlHelper helper, string key)
        {
            return new HtmlString(I18NEntity.GetString(key));
        }

        public static HtmlString I18N(this IHtmlHelper helper, string key, params object[] args)
        {
            return new HtmlString(I18NEntity.GetString(key, args));
        }
    }
}