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
using System.Web;
using AvePoint.Wrapper.Common;
using System.Diagnostics.CodeAnalysis;

namespace AvePoint.Wrapper.Restore
{
    class URLDataFormat:BaseDataFormat
    {
        private string description;
        private int originalVersion;

        public URLDataFormat(AveXmlField xmlField, IAveField destField, AveSPItem mItem, string description,int originalVersion) :
            base(xmlField, destField, mItem)
        {
            this.description = description;
            this.originalVersion = originalVersion;
        }
        public override object CheckFieldValue(object value)
        {
            return GetUrlValue(value, mItem.RowId);
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "/_catalogs/masterpage")]
        private IAveFieldUrlValue GetUrlValue(object value, int docRowId)
        {
            bool needSiteCollectionLevel = mItem.ParentList.SPList != null
                && (mItem.ParentList.SPList.BaseTemplate == AveListTemplateType.DesignCatalog
                     && (base.destField.InternalName.Equals("ThemeUrl", StringComparison.Ordinal)
                         || base.destField.InternalName.Equals("FontSchemeUrl", StringComparison.Ordinal)
                        )
                    );
            var urlValue = mItem.ParentSite.ObjectModelFactory.CreateFieldUrlValue();
            string url = value.ToString();
            //此IF判断URL是否指向源端site里的Content. 1.SourceSite的URL肯定不以'/'结尾。 2.必须在site url末尾加上'/', 以完全确定URL是否指向源端site里的Content。 ADO-175420
            if (url.Equals(mItem.ParentSite.SourceSiteInfo.Url, StringComparison.OrdinalIgnoreCase)
                || url.StartsWith(mItem.ParentSite.SourceSiteInfo.Url + "/", StringComparison.OrdinalIgnoreCase))
            {
                if (mItem.ParentSite.SourceSiteInfo.ServerRelativeUrl.Equals("/", StringComparison.OrdinalIgnoreCase))
                {
                    url = url.Substring(mItem.ParentSite.SourceSiteInfo.Url.Length);
                }
                else
                {
                    url = url.Replace(mItem.ParentSite.SourceSiteInfo.Url, mItem.ParentSite.SourceSiteInfo.ServerRelativeUrl);
                }
            }
            urlValue.Url = ReplaceUrl(docRowId, base.destField, url, needSiteCollectionLevel && url.IndexOf("/_catalogs/theme/", StringComparison.OrdinalIgnoreCase) > 0);

            if (HttpUtility.UrlDecode(description).Equals(HttpUtility.UrlDecode(url), StringComparison.OrdinalIgnoreCase))
            {
                description = urlValue.Url;
            }
            else if (description.StartsWith("http:", StringComparison.OrdinalIgnoreCase) ||
                     description.StartsWith("https:", StringComparison.OrdinalIgnoreCase))
            {
                description = ReplaceUrl(docRowId, base.destField, description, needSiteCollectionLevel && description.IndexOf("/_catalogs/theme/", StringComparison.OrdinalIgnoreCase) > 0);
            }
            urlValue.Description = description;

            //wiki page中的PublishingPageLayout，应该指向的是root site上masterpage中的文件,当做root site到sub site的mapping时，
            //UrlReplace替换成指向sub site上的masterpage，导致还原后的wiki page打开出错，在此处理这种case，
            //将PublishingPageLayout指向的Url换成指向root site上masterpage中的文件。
            if (base.destField.InternalName == "PublishingPageLayout")
            {
                if (urlValue.Url.Contains("/_catalogs/masterpage"))
                {
                    string temUrl = mItem.ParentSite.SPSite.ServerRelativeUrl.TrimEnd('/') +
                                    "/_catalogs/masterpage";
                    if (!urlValue.Url.StartsWith(temUrl, StringComparison.OrdinalIgnoreCase))
                    {
                        urlValue.Url = mItem.ParentSite.SPSite.ServerRelativeUrl.TrimEnd('/') +
                                       urlValue.Url.Substring(
                                           urlValue.Url.IndexOf("/_catalogs/masterpage",
                                                                StringComparison.OrdinalIgnoreCase));
                    }
                }
            }
            return urlValue;
        }

        private string ReplaceUrl(int docRowId, IAveField spField, string url, bool siteCollectionLevel)
        {
            using (new AvePerformanceScope("Restore.SetFieldValueURL.ReplaceUrl"))
            {
                if (siteCollectionLevel)
                {
                    url = AveReplaceProcessor.UrlReplace(url, mItem.ParentWeb.ParentSite.MappingManager.SiteMappingManager.SiteUrlMapping,
                                                                 new ReplaceOption(true, true), mItem.ParentSite.SourceSiteInfo, mItem.ParentSite.ServerRelativeUrl);
                }
                else
                {
                    url = AveReplaceProcessor.UrlReplace(url, mItem.ParentWeb.ParentSite.MappingManager.SiteMappingManager.SiteManagedMappings,
                                                                 new ReplaceOption(true, true), mItem.ParentSite.SourceSiteInfo, mItem.ParentSite.ServerRelativeUrl);
                }
                Guid sourceItemId;
                if (AveUrlUtility.IsDurableLink(url, out sourceItemId))
                {
                    string mappedUrl;
                    if (!mItem.ParentWeb.ParentSite.MappingManager.SiteMappingManager.TryGetDurableLinkUrl(sourceItemId, out mappedUrl))
                    {
                        var urlFieldInfo = new AveUrlFieldInfo
                        {
                            FieldId = spField.ID,
                            SourceItemId = sourceItemId,
                            Version = originalVersion
                        };
                        mItem.ParentList.AveFields.ResetUrlFieldValues(urlFieldInfo);

                    }
                    else
                    {
                        url = mappedUrl;
                    }
                }
                else if (url.Contains("?")) //替换Url中的Id
                {
                    bool needReplaceLast = false;
                    url = AveReplaceProcessor.IdReplace(url, mItem.ParentWeb.ParentSite.MappingManager, ref needReplaceLast);
                    if (needReplaceLast)
                    {
                        mItem.ParentWeb.ParentSite.AddUnReplaceUrlIDCache(mItem.ParentWeb.SPWeb.ID,
                                                                    mItem.ParentList.SPList.ID, docRowId,
                                                                    spField.InternalName);
                    }
                }
                return url;
            }
        }
    }
}
