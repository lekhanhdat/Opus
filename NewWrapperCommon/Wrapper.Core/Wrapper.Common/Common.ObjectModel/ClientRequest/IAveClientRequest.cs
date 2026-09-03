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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvePoint.Wrapper.Common
{
    public interface IAveClientRequest : IDisposable
    {
        AveServerVersion SPVersion { get; }

        string SiteUrl { get; }
        
        AveAuthenticationMode AveAuthMode { get; }

        List<Dictionary<string, object>> LoadPersonalSiteInfosForUsers(List<string> usernames);

        Dictionary<string, object> GetBrowserSiteInfo();

        void AddSiteAdmin(string username, string siteCollectionUrl, string tenantAdminSiteUrl);

        Dictionary<string, object> GetUsers(string url, string groupName, string scope);

        Dictionary<string, object> GetSite();

        Dictionary<string, object> GetUser(string userEmail);
        Dictionary<string, object> GetWebTemplates(string webServerRelativeUrl, uint lcid, bool doIncludeCrossLanguage, string webtemplateSource);
    }

    public static class AuthenticationModeOptionDefaultValue
    {
        public static AuthenticationModeOption[] DefaultValue = new AuthenticationModeOption[] { AuthenticationModeOption.Windows, AuthenticationModeOption.Online, AuthenticationModeOption.Forms, AuthenticationModeOption.Claims, AuthenticationModeOption.ADFS };
    }

    public enum AuthenticationModeOption
    {
        Windows,
        Claims,
        Online,
        Forms,
        ADFS,
        /// <summary>
        /// 请注意： 此Option获取到的Token只能用于Graph API。所以不要把它加到Default Option里，有需要时单独使用。
        /// </summary>
        OnlineGraphToken
    }
}
