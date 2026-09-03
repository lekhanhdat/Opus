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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using AvePoint.Common;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.CodeReview;
using AvePoint.GCommon.Utility.I18N;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Mapping;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.GCommon.Utility.Exceptions.SharePoint;

namespace AvePoint.Wrapper.Restore
{
    class URLValueConvertObject : BaseValueConvertObject
    {
        private string description;
        private int originalVersion;
        public URLValueConvertObject(IAveField destField, AveSPItem mItem, int originalRowId, string description,int originalVersion)
            : base(destField, mItem, originalRowId)
        {
            this.description = description;
            this.originalVersion = originalVersion;
        }

        public override object ConvertSingleValue(string value)
        {
            return GetUrlValue(value, originalRowId);
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "/_catalogs/masterpage")]
        private IAveFieldUrlValue GetUrlValue(string value, int docRowId)
        {
            bool needSiteCollectionLevel = mItem.ParentList.SPList != null
                && (mItem.ParentList.SPList.BaseTemplate == AveListTemplateType.DesignCatalog
                     && (base.destField.InternalName.Equals("ThemeUrl", StringComparison.Ordinal)
                         || base.destField.InternalName.Equals("FontSchemeUrl", StringComparison.Ordinal)
                        )
                    );
            var urlValue = mItem.ParentSite.ObjectModelFactory.CreateFieldUrlValue();
            string url = value;
            if (url.StartsWith(mItem.ParentSite.SourceSiteInfo.Url, StringComparison.OrdinalIgnoreCase))
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
                var descriptionInfo = new AveSourceFieldValueInfo
                {
                    SourceFieldInfo = new AveSourceFieldInfo {
                        SourceInternalName = destField.InternalName + "#2",
                        SourceDisplayName = destField.Title + "#2",
                    },
                    SourceValue = urlValue.Description,
                };
                urlValue.Description = mItem.ParentList.AveFields.FieldMapping.GetValueFromGuiMapping(descriptionInfo);
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
