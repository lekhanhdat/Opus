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
namespace AvePoint.ObjectModel.ClientOM
{
    using System.Collections.Generic;
    using System.Linq;
    using System.IO;
    using Microsoft.Online.SharePoint.TenantAdministration;
    using Microsoft.SharePoint.Client;
    using System;
    using AvePoint.GCommon;

    class ContextCacheDisableScope:IDisposable
    {
        private ClientContext mContext;
        private bool OriginalValue;
        public ContextCacheDisableScope(ClientContext context)
        {
            mContext = context;
            OriginalValue = mContext.DisableReturnValueCache;
            mContext.DisableReturnValueCache = true;
        }

        public void Dispose()
        {
            mContext.DisableReturnValueCache = OriginalValue;
        }
    }

    static class TenantExtension
    {
        public static IEnumerable<SiteProperties> GetSitePropertiesNew(this Tenant tenant,Action<SPOSitePropertiesEnumerable> loadAction, bool? includeODFB, bool includeDetail=false, string startIndex=null, string filter=null,string template=null, int groupIdDefined=0)
        {
            SPOSitePropertiesEnumerableFilter speFilter = new SPOSitePropertiesEnumerableFilter
            {
                Filter = filter,
                GroupIdDefined = groupIdDefined,
                IncludeDetail = includeDetail,
                IncludePersonalSite = includeODFB.HasValue ? includeODFB.Value ? PersonalSiteFilter.Include : PersonalSiteFilter.Exclude : PersonalSiteFilter.UseServerDefault,
                StartIndex=startIndex,
                Template=template
            };
            SPOSitePropertiesEnumerable siteProperties= null;
            do
            {
                siteProperties = tenant.GetSitePropertiesFromSharePointByFilters(speFilter);
                loadAction?.Invoke(siteProperties);
                tenant.Context.ExecuteQuery();
                speFilter.StartIndex = siteProperties.NextStartIndexFromSharePoint;
                foreach (var site in siteProperties)
                {
                    yield return site;
                }
            }
            while (siteProperties != null && siteProperties.NextStartIndexFromSharePoint != null);
        }

        public static IEnumerable<SiteProperties> GetSitePropertiesOriginal(this Tenant tenant, Action<SPOSitePropertiesEnumerable> loadAction, bool includeDetail = false, int startIndex = 0, string filter = null)
        {
            if (string.IsNullOrEmpty(filter))
            {
                foreach (var site in tenant.GetSitePropertiesOriginal(loadAction, includeDetail, startIndex))
                {
                    yield return site;
                }
            }
            else
            {
                SPOSitePropertiesEnumerable siteProperties = null;
                int nextStartIndex = startIndex;

                do
                {
                    siteProperties = tenant.GetSitePropertiesByFilter(filter, nextStartIndex, includeDetail);
                    loadAction?.Invoke(siteProperties);
                    tenant.Context.ExecuteQuery();
                    nextStartIndex = siteProperties.NextStartIndex;
                    foreach (var site in siteProperties)
                    {
                        yield return site;
                    }
                }
                while (siteProperties != null && siteProperties.NextStartIndex > 0);
            }
        }

        public static IEnumerable<SiteProperties> GetSitePropertiesOriginal(this Tenant tenant, Action<SPOSitePropertiesEnumerable> loadAction, bool includeDetail = false, int startIndex = 0)
        {
            SPOSitePropertiesEnumerable siteProperties = null;
            int nextStartIndex = startIndex;

            do
            {
                siteProperties = tenant.GetSiteProperties( nextStartIndex, includeDetail);
                loadAction?.Invoke(siteProperties);
                tenant.Context.ExecuteQuery();
                nextStartIndex = siteProperties.NextStartIndex;
                foreach (var site in siteProperties)
                {
                    yield return site;
                }
            }
            while (siteProperties != null && siteProperties.NextStartIndex > 0);
        }

    }
    static class ClientExtension
    {
        private static AveLogger mLogger = AveLogger.GetInstance(typeof(ClientExtension));

        public static Microsoft.SharePoint.Client.File AddUsingPathV1(this FileCollection fileCollection, ResourcePath path, FileCollectionAddParameters parameters, Stream contentStream)
        {
            var file = fileCollection.AddUsingPath(path, parameters, contentStream);

            //--CSOM Bug--, need to remove this if this issue is fixed.
            file.Path.SetPendingReplace();
            ObjectIdentityQuery objectIdentityQuery = new ObjectIdentityQuery(file.Path);
            fileCollection.Context.AddQueryIdAndResultObject(objectIdentityQuery.Id, file);
            fileCollection.Context.AddQuery(objectIdentityQuery);

            return file;
        }

        public static void RetrieveSiteProperties(this SiteProperties siteProperties)
        {
            var properties = typeof(SitePropertiesPropertyNames).GetFields().Select(t => t.GetValue(null).ToString()).ToList();
            if (properties.Contains(SitePropertiesPropertyNames.SensitivityLabel))
            {
                properties.Remove(SitePropertiesPropertyNames.SensitivityLabel);
            }
            //For new CSOM api
            if (properties.Contains("NewUrl"))
            {
                properties.Remove("NewUrl");
            }
            //SAAS-40525 readonly site will be accessed failed.
            if (properties.Contains("AllowExternalEmbeddingWrapper"))
            {
                properties.Remove("AllowExternalEmbeddingWrapper");
            }
            if (properties.Contains("AllowedExternalDomains"))
            {
                properties.Remove("AllowedExternalDomains");
            }
            if (properties.Contains("CustomizedFormsPages"))
            {
                properties.Remove("CustomizedFormsPages");
            }
            if (properties.Contains("LoopOverridesharingcapability"))
            {
                properties.Remove("LoopOverridesharingcapability");
            }
            if (properties.Contains("LoopSharingCapability"))
            {
                properties.Remove("LoopSharingCapability");
            }

            siteProperties.Retrieve(properties.ToArray());
        }

        public static void RetrieveSite(this Site site)
        {
            var properties = typeof(SitePropertyNames).GetFields().Select(t => t.GetValue(null).ToString()).ToList();
            string result = string.Join(", ", properties);
            mLogger.Info($"RetrieveSite.Properties:{result}.");
            if (properties.Contains(SitePropertyNames.SensitivityLabel, StringComparer.OrdinalIgnoreCase))
            {
                properties.Remove(SitePropertyNames.SensitivityLabel);
            }
            //SAAS-40525 readonly site will be accessed failed.
            if (properties.Contains("AllowExternalEmbeddingWrapper"))
            {
                properties.Remove("AllowExternalEmbeddingWrapper");
            }
            if (properties.Contains("CustomizedFormsPages"))
            {
                properties.Remove("CustomizedFormsPages");
            }
            if (properties.Contains("AllowedExternalDomains"))
            {
                properties.Remove("AllowedExternalDomains");
            }
            if (properties.Contains("LoopOverridesharingcapability"))
            {
                properties.Remove("LoopOverridesharingcapability");
            }
            if (properties.Contains("LoopSharingCapability"))
            {
                properties.Remove("LoopSharingCapability");
            }

            site.Retrieve(properties.ToArray());
        }
    }
}
