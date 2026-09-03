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
using AvePoint.Wrapper.Common;
using System.Diagnostics.CodeAnalysis;

namespace AvePoint.ObjectModel.Common
{
    class AveThmxTheme : AveClientObject, IAveThmxTheme
    {
        private AveSite mSite;
        private AveWeb mWeb;
        private IAveRequest mRequest;

        public AveThmxTheme(IAveSite site)
        {
            mSite = site as AveSite;
            mRequest = mSite.Request;
        }
        
        public string DarkColor1
        {
            get 
            {
                return base.DataCache.GetProperty<string>("DarkColor1");
            }
        }

        public string DarkColor2
        {
            get
            {
                return base.DataCache.GetProperty<string>("DarkColor2");
            }
        }
        
        public string LightColor1
        {
            get
            {
                return base.DataCache.GetProperty<string>("LightColor1");
            }
        }
        
        public string LightColor2
        {
            get
            {
                return base.DataCache.GetProperty<string>("LightColor2");
            }
        }
        
        public string AccentColor1
        {
            get
            {
                return base.DataCache.GetProperty<string>("AccentColor1");
            }
        }

        public string AccentColor2
        {
            get
            {
                return base.DataCache.GetProperty<string>("AccentColor2");
            }
        }

        public string AccentColor3
        {
            get
            {
                return base.DataCache.GetProperty<string>("AccentColor3");
            }
        }
        
        public string AccentColor4
        {
            get
            {
                return base.DataCache.GetProperty<string>("AccentColor4");
            }
        }
        
        public string AccentColor5
        {
            get
            {
                return base.DataCache.GetProperty<string>("AccentColor5");
            }
        }
        
        public string AccentColor6
        {
            get
            {
                return base.DataCache.GetProperty<string>("AccentColor6");
            }
        }
        
        public string HyperlinkColor
        {
            get
            {
                return base.DataCache.GetProperty<string>("HyperlinkColor");
            }
        }
        
        public string FollowedHyperlinkColor
        {
            get
            {
                return base.DataCache.GetProperty<string>("FollowedHyperlinkColor");
            }
        }
        
        public string MajorFont
        {
            get
            {
                return base.DataCache.GetProperty<string>("MajorFont");
            }
        }
        
        public string MinorFont
        {
            get
            {
                return base.DataCache.GetProperty<string>("MinorFont");
            }
        }

        public AveThmxTheme(IAveSite site, Dictionary<string, object> thmxThemeProperties)
            : this(site)
        {
            base.DataCache.AddPropertyies(thmxThemeProperties);
        }

        public string Name
        {
            get
            {
                string name = base.DataCache.GetProperty<string>("Name");
                if (name.EndsWith(".thmx",StringComparison.OrdinalIgnoreCase))
                {
                    return name.Substring(0, name.IndexOf('.'));
                }
                return name;
            }
            set
            {
                base.DataCache.AddChangedProperty("Name", value);
            }
        }

        public string ServerRelativeUrl
        {
            get 
            {
                return base.DataCache.GetProperty<string>("ServerRelativeUrl");
            }
        }

        public IAveFile File
        {
            get 
            {
                return null;
            }
        }

        public void ApplyTo(IAveWeb web, bool shareGenerated)
        {
            //mRequest.ApplyTo(web.ServerRelativeUrl, shareGenerated, this.Name);
            AveWebThemeInfo themeInfo = new AveWebThemeInfo();
            themeInfo.ThemeName = this.Name;
            if (web.AllProperties.ContainsKey("__InheritsThemedCssFolderUrl"))
            {
                themeInfo.InheritsThemedCssFolderUrl = Convert.ToBoolean(web.AllProperties["__InheritsThemedCssFolderUrl"].ToString());
            }
            else
            {
                themeInfo.InheritsThemedCssFolderUrl = false;
            }
            string siteServerRelativeUrl = string.Empty;
            if (!web.IsRootWeb)
            {
                siteServerRelativeUrl = web.Site.ServerRelativeUrl;
            }
            AveWebSettingInfo settingInfo = new AveWebSettingInfo();
            settingInfo.WebTheme = themeInfo;
            mRequest.RestoreTheme(web.ServerRelativeUrl, siteServerRelativeUrl, settingInfo, string.Empty);
        }

        public void Close()
        {
            
        }

        public System.Collections.ObjectModel.ReadOnlyCollection<IAveThmxTheme> GetManagedThemes(IAveSite site)
        {
            return (site as AveSite).ManagedThemes;            
        }

        public string GetThemeUrlForWeb(IAveWeb web)
        {
            Dictionary<string, object> themeDic = mRequest.GetThemeUrlForWeb(web.ServerRelativeUrl, web.Site.CompatibilityLevel);
            if (themeDic.ContainsKey("ThemeUrl"))
            {
                return themeDic["ThemeUrl"].ToString();
            }
            return null;
        }

        public IAveThmxTheme Open(IAveSite site, string url)
        {
            Dictionary<string, object> themeProp = mRequest.OpenThmxTheme(url);
            if (themeProp.Count > 0)
            {
                return new AveThmxTheme(site, themeProp);
            }
            return null;
        }

        public void RemoveThemeFromWeb(IAveWeb web, bool deleteFiles)
        {
            mRequest.RemoveThemeFromWeb(web.ServerRelativeUrl, deleteFiles);
        }

        public void SetThemeUrlForWeb(IAveWeb web, string themeUrl)
        {
           //Can not support this method, remove empy method from request
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "sitecollection is a part of url")]
        internal static string GetThemeGalleryUrl(IAveSite site)
        {
            return UrlFromPrefixedUrlCore("~sitecollection/_catalogs/theme", null, site.ServerRelativeUrl);
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "sitecollection is a part of value")]
        internal static string UrlFromPrefixedUrlCore(string prefixedUrl, string webUrl, string siteUrl)
        {
            string webRelativeUrlPrefix = "~site/";
            string siteRelativeUrlPrefix = "~sitecollection/";
            string contextLayoutsFolder = "_layouts/15";
            if (((prefixedUrl == null) || (prefixedUrl.Length <= 1)) || (prefixedUrl[0] != '~'))
            {
                return prefixedUrl;
            }
            StringBuilder builder = new StringBuilder(prefixedUrl.Length);
            int startIndex = -1;
            if (prefixedUrl.StartsWith(webRelativeUrlPrefix, StringComparison.OrdinalIgnoreCase))
            {
                if (webUrl == null)
                {
                    builder.Append(siteUrl);//(SPContext.Current.Web.ServerRelativeUrl);
                }
                else
                {
                    builder.Append(webUrl);
                }
                startIndex = webRelativeUrlPrefix.Length;
            }
            else if (prefixedUrl.StartsWith(siteRelativeUrlPrefix, StringComparison.OrdinalIgnoreCase))
            {
                if (siteUrl == null)
                {
                    builder.Append(string.Empty);//(SPContext.Current.Site.ServerRelativeUrl);
                }
                else
                {
                    builder.Append(siteUrl);
                }
                startIndex = siteRelativeUrlPrefix.Length;
            }
            else if (prefixedUrl.StartsWith("~layouts/", StringComparison.OrdinalIgnoreCase))
            {
                builder.Append(contextLayoutsFolder);
                startIndex = "~layouts/".Length;
            }
            else if (prefixedUrl.StartsWith("~siteLayouts/", StringComparison.OrdinalIgnoreCase))
            {
                if (webUrl == null)
                {
                    builder.Append(siteUrl);//(SPContext.Current.Web.ServerRelativeUrl);
                }
                else
                {
                    builder.Append(webUrl);
                }
                if ((builder.Length <= 0) || (builder[builder.Length - 1] != '/'))
                {
                    builder.Append('/');
                }
                builder.Append(contextLayoutsFolder);
                startIndex = "~siteLayouts/".Length;
            }
            else
            {
                if (!prefixedUrl.StartsWith("~siteCollectionLayouts/", StringComparison.OrdinalIgnoreCase))
                {
                    return prefixedUrl;
                }
                if (siteUrl == null)
                {
                    builder.Append(string.Empty);//(SPContext.Current.Site.ServerRelativeUrl);
                }
                else
                {
                    builder.Append(siteUrl);
                }
                if ((builder.Length <= 0) || (builder[builder.Length - 1] != '/'))
                {
                    builder.Append('/');
                }
                builder.Append(contextLayoutsFolder);
                startIndex = "~siteCollectionLayouts/".Length;
            }
            if ((builder.Length <= 0) || (builder[builder.Length - 1] != '/'))
            {
                builder.Append('/');
            }
            builder.Append(prefixedUrl.Substring(startIndex));
            return builder.ToString();

        }

        public void Dispose()
        {
            
        }
    }
}
