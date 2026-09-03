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

namespace AvePoint.Wrapper.Common
{
    using System;
    using System.Collections.Generic;

    public struct AveSPWebTemplate
    {
        public const string ONE_DRIVE_TEMPLATE = "SPSPERS#";
        /// <summary>
        /// This template is used for initializing a new site.
        /// </summary>
        public const string GLOBAL_TEMPLATE = "GLOBAL#0";

        public const string CUSTOMIZED = "STS";
        public const string MEETING_WORKSPACE = "MPS";

        /// <summary>
        /// A site with a classic experience on the home page and no connection to an Office 365 Group.
        /// </summary>
        public const string TEAM_SITE_CLASSIC = "STS#0";

        /// <summary>
        /// A site with no connection to an Office 365 Group.
        /// </summary>
        public const string TEAM_SITE_NO_GROUP = "STS#3";

        /// <summary>
        /// A site for a community to brainstorm and share ideas. It provides Web pages that can be quickly edited to record information and then linked together through keywords
        /// </summary>
        public const string WIKI_SITE = "WIKI#0";

        public const string DOCUMENT_CENTER = "BDR#0";
        public const string BLOG = "BLOG#0";

        #region retired classic publishing site
        /// <summary>
        /// A starter site hierarchy for an Internet-facing site or a large intranet portal. This site can be customized easily with distinctive branding. 
        /// It includes a home page, a sample press releases subsite, a Search Center, and a login page. Typically, this site has many more readers than contributors, and it is used to publish Web pages with approval workflows.
        /// </summary>
        public const string PUBLISHING_PORTAL = "BLANKINTERNETCONTAINER#0";

        /// <summary>
        /// This template creates a site for publishing Web pages on a schedule, with workflow features enabled.  By default, only Publishing subsites can be created under this site. 
        /// A Document and Picture Library are included for storing Web publishing assets.
        /// </summary>
        public const string PUBLISHING_BLANK_SITE = "BLANKINTERNET#0";

        public const string PUBLISHING_SITE = "CMSPUBLISHING#0";
        public const string ENTERPRISE_WIKI_SITE = "ENTERWIKI#0";
        public const string ENTERPRISE_SEARCH_CENTER = "SRCHCEN#0";
        public const string SITE_DIRECTORY_SITE = "SPSSITES#0";
        public const string NEWS_HOME_SITE = "SPSNHOME#0";
        public const string PRODUCT_CATALOG_SITE = "PRODUCTCATALOG#0";
        public const string REPORT_CENTER_SITE = "SPSREPORTCENTER#0";
        public const string TOPIC_AREA_TEMPLATE_SITE = "SPSTOPIC#0";
        #endregion

        public const string SHAREPOINTEMBEDDED_SITE = "CSPCONTAINER#0";
        public const string COMMUNITY_SITE = "COMMUNITY#0";
        public const string COMMUNITY_PORTAL = "COMMUNITYPORTAL#0";

        /// <summary>
        /// Share documents, have conversations with your team, keep track of events, manage tasks, and more with a site connected to an Office 365 Group.
        /// </summary>
        public const string TEAM_SITE_GROUP = "GROUP#0";

        /// <summary>
        /// Microsoft Teams Channel Site.
        /// </summary>
        public const string TEAM_CHANNEL = "TEAMCHANNEL#";

        public const string COMMUNICATION_SITE = "SITEPAGEPUBLISHING#0";

        /// <summary>
        /// A site that supports team collaboration on projects. This site includes documents, issues, risks, and deliverables which may be linked to tasks in Project Web App.
        /// </summary>
        public const string PROJECT_SITE = "PWS#0";

        public const string PROJECT_WEB_APP_SITE = "PWA#0";

        public static bool IsCustomized(string webTemplate)
        {
            return string.Equals(CUSTOMIZED, webTemplate, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Team site now can assoicate with group via Connect to new Microsoft 365 Group
        /// Cannot distinguish if it's group team site only by site template.
        /// </summary>
        /// <param name="webTemplate"></param>
        /// <param name="groupSiteEmail"></param>
        /// <returns></returns>
        public static bool IsGroupTeamSite(string webTemplate, string groupSiteEmail)
        {
            return string.Equals(TEAM_SITE_GROUP, webTemplate, StringComparison.OrdinalIgnoreCase)
                || (!string.IsNullOrEmpty(groupSiteEmail) && !IsOneDrive(webTemplate) && !IsTeamPrivateChannelSite(webTemplate));
        }

        public static bool IsTeamPrivateChannelSite(string webTemplate)
        {
            return webTemplate.StartsWith(TEAM_CHANNEL, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsPwaSite(string webTemplate)
        {
            return string.Equals(PROJECT_WEB_APP_SITE, webTemplate, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsTeamSiteNoGroup(string webTemplate)
        {
            return string.Equals(TEAM_SITE_NO_GROUP, webTemplate, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsProjectSite(string webTemplate)
        {
            return string.Equals(PROJECT_SITE, webTemplate, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsOneDrive(string webTemplate)
        {
            return webTemplate.StartsWith(ONE_DRIVE_TEMPLATE, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsModernThemeSite(string webTemplate)
        {
            return string.Equals(COMMUNICATION_SITE, webTemplate, StringComparison.OrdinalIgnoreCase)
                || string.Equals(TEAM_SITE_GROUP, webTemplate, StringComparison.OrdinalIgnoreCase)
                || string.Equals(TEAM_SITE_NO_GROUP, webTemplate, StringComparison.OrdinalIgnoreCase)
                || IsTeamPrivateChannelSite(webTemplate)
                || string.Equals(TEAM_SITE_CLASSIC, webTemplate, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Determines if the specified web template is a communication site.
        /// </summary>
        /// <param name="webTemplate"></param>
        /// <returns></returns>
        public static bool IsCommunicationSite(string webTemplate)
        {
            return string.Equals(COMMUNICATION_SITE, webTemplate, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Determines if the specified web template is a retired classic publishing site.
        /// </summary>
        /// <param name="webTemplate"></param>
        /// <returns></returns>
        public static bool IsRetiredClassicPublishingSite(string webTemplate)
        {
            List<string> retiredClassicPublishingSiteTemplates =
            [
                PUBLISHING_PORTAL,
                PUBLISHING_BLANK_SITE,
                PUBLISHING_SITE,
                ENTERPRISE_WIKI_SITE,
                ENTERPRISE_SEARCH_CENTER,
                SITE_DIRECTORY_SITE,
                NEWS_HOME_SITE,
                PRODUCT_CATALOG_SITE,
                REPORT_CENTER_SITE,
                TOPIC_AREA_TEMPLATE_SITE
            ];

            return retiredClassicPublishingSiteTemplates.Exists(template => template.EqualsIgnoreCase(webTemplate));
        }
    }
}