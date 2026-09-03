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

namespace Microsoft365.SharePoint.Rest.HubSite
{
    using Microsoft365.Authentication.TokenProvider;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;

    /// <summary>
    /// The SharePoint REST interface to register sites as hub sites, associate existing sites with hub sites, and obtain or update information about hub sites.
    /// </summary>
    public class HubSiteService
    {
        public string SiteUrl { get; private set; }
        private readonly SharePointRestExecutor executor;

        /// <summary>
        /// </summary>
        /// <param name="siteUrl">Target site url</param>
        /// <param name="tokenProvider">Token provider to obtain access token. If user token is used, it requires SharePoint admin and site admin permission.</param>
        public HubSiteService(string siteUrl, IATokenProvider tokenProvider)
        {
            _ = siteUrl ?? throw new ArgumentNullException(nameof(SiteUrl));
            _ = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));

            this.SiteUrl = siteUrl.TrimEnd('/');
            this.executor = new SharePointRestExecutor(this.SiteUrl, tokenProvider, true);
            this.executor.MaxDataServiceVersion = "3.0";
        }


        /// <summary>
        /// Registers an existing site as a hub site.
        /// </summary>
        /// <returns>Data describing a SharePoint hub site.</returns>
        public SPHubSite RegisterHubSite()
        {
            var requestUrl = $"{this.SiteUrl}/_api/site/RegisterHubSite";
            return this.executor.Post<SPHubSite>(new Uri(requestUrl), null);
        }

        /// <summary>
        /// Unregisters a hub site so that it is no longer a hub site. It will become a regular site. Any sites associated with the hub site will no longer be associated. This can take up to an hour to propagate.
        /// </summary>
        public void UnRegisterHubSite() 
        {
            var requestUrl = $"{this.SiteUrl}/_api/site/UnRegisterHubSite";
            this.executor.Post<SPHubSite>(new Uri(requestUrl), null);
        }

        //public IEnumerable<SPHubSite> HubSites()
        //{ }

        /// <summary>
        /// Gets information about a hub site
        /// </summary>
        /// <param name="siteId">The ID of the hub site to get information about.</param>
        /// <returns></returns>
        public SPHubSite GetHubSiteById(Guid siteId)
        {
            if (siteId == Guid.Empty) throw new ArgumentNullException(nameof(siteId));

            var requestUrl = $"{this.SiteUrl}/_api/hubsites/getbyid?hubSiteId='{siteId.ToString().ToLowerInvariant()}'";
            return this.executor.Get<SPHubSite>(new Uri(requestUrl), null);
        }

        /// <summary>
        /// Updates information about a hub site
        /// </summary>
        /// <param name="siteId">The ID of the hub site to get information about.</param>
        /// <param name="hubSite">
        /// For string value, set it to null will not perform any update, set it to string.Empty instead. 
        /// Support the following properties, other properties will be ingnored.
        /// Description,
        /// EnablePermissionsSync
        /// HideNameInNavigation
        /// LogoUrl
        /// Targets 
        /// Title</param>
        public void UpdateHubSiteById(Guid siteId,SPHubSite hubSite) 
        {
            if (siteId == Guid.Empty) throw new ArgumentNullException(nameof(siteId));
            _ = hubSite ?? throw new ArgumentNullException(nameof(hubSite));

            var hubToPost = TrimPropertiesForPost(hubSite);
            var requestUrl = $"{this.SiteUrl}/_api/hubsites/getbyid?hubSiteId='{siteId.ToString().ToLowerInvariant()}'";
            var header = new Dictionary<string, string> { { "X-HTTP-Method", "MERGE" },{ "if-Match", "*"} };
            this.executor.Post<SPHubSite>(new Uri(requestUrl), hubToPost, header);
        }

        private SPHubSite TrimPropertiesForPost(SPHubSite hubSite)
        {
            return new SPHubSite
            {
                Description = hubSite.Description,
                EnablePermissionsSync = hubSite.EnablePermissionsSync,
                HideNameInNavigation = hubSite.HideNameInNavigation,
                LogoUrl = hubSite.LogoUrl,
                //ParentHubSiteId = hubSite.ParentHubSiteId,
                //RequiresJoinApproval = hubSite.RequiresJoinApproval,
                //SiteDesignId = hubSite.SiteDesignId,
                Targets = hubSite.Targets,
                Title = hubSite.Title,
            };
        }

        //public SPHubSiteData GetHubSiteData() 
        //{

        //}

        /// <summary>
        /// Associates a site with an existing hub site. You can also use this method to disassociate a site from a hub site(set hubSiteId to Guid.Empty)
        /// </summary>
        /// <param name="hubSiteId"></param>
        public void JoinHubSite(Guid hubSiteId) 
        {
            var requestUrl = $"{this.SiteUrl}/_api/site/JoinHubSite('{hubSiteId.ToString().ToLowerInvariant()}')";
            this.executor.Post<SPHubSite>(new Uri(requestUrl), null);
        }

        public void RefreshHubSiteData()
        {
            var requestUrl = $"{this.SiteUrl}/_api/web/hubsitedataasstream(true)";
            this.executor.Post<SPHubSiteData>(new Uri(requestUrl), null);
        }

    }
}
