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

using AvePoint.Wrapper.Common;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvePoint.ObjectModel.ServerSE
{
    public class AveTheme : IAveTheme
    {
        private SPTheme mTheme;

        public AveTheme()
        {
        }

        public AveTheme(SPTheme theme)
        {
            if (theme == null)
            {
                throw new ArgumentNullException("theme");
            }
            mTheme = theme;
        }

        internal SPTheme Theme
        {
            get
            {
                return mTheme;
            }
        }

        public string AccessibleDescription
        {
            get
            {
                if (mTheme == null)
                {
                    return null;
                }
                return mTheme.AccessibleDescription;
            }
        }

        public Uri BackgroundImageUri
        {
            get
            {
                if (mTheme == null)
                {
                    return null;
                }
                return mTheme.BackgroundImageUri;
            }
        }

        public bool IsInverted
        {
            get
            {
                if (mTheme == null)
                {
                    return false;
                }
                return mTheme.IsInverted;
            }
        }

        public string Name
        {
            get
            {
                if (mTheme == null)
                {
                    return null;
                }
                return mTheme.Name;
            }
        }

        public void ApplyTo(IAveWeb web, bool shareGenerated)
        {
            if (mTheme == null)
            {
                return;
            }
            SPWeb spWeb = null;
            if (web != null)
            {
                spWeb = (web as AveWeb).Web;
            }
            mTheme.ApplyTo(spWeb, shareGenerated);
        }

        public void EnforceThemedStylesForWeb(IAveWeb web)
        {
            SPWeb spWeb = null;
            if (web != null)
            {
                spWeb = (web as AveWeb).Web;
            }
            SPTheme.EnforceThemedStylesForWeb(spWeb);
        }

        public bool EnsureThemedStylesForLocales(IAveWeb web, HashSet<int> languages)
        {
            SPWeb spWeb = null;
            if (web != null)
            {
                spWeb = (web as AveWeb).Web;
            }
            return SPTheme.EnsureThemedStylesForLocales(spWeb, languages);
        }

        public IAveThemeFont GetThemeFontByName(string fontName)
        {
            if (mTheme == null)
            {
                return null;
            }
            return new AveThemeFont(mTheme.GetThemeFontByName(fontName));
        }

        public void OnPostApplyTheme(IAveWeb web, string newThemedCssFolderUrl, string oldThemedCssFolderUrl)
        {
            SPWeb spWeb = null;
            if (web != null)
            {
                spWeb = (web as AveWeb).Web;
            }
            SPTheme.OnPostApplyTheme(spWeb, newThemedCssFolderUrl, oldThemedCssFolderUrl);
        }

        public IAveTheme Open(string name, IAveFile colorPaletteFile)
        {
            SPFile spFile = null;
            if (colorPaletteFile != null)
            {
                spFile = (colorPaletteFile as AveFile).File;
            }
            SPTheme sPTheme = SPTheme.Open(name, spFile);
            return new AveTheme(sPTheme);
        }

        public IAveTheme Open(string name, IAveFile colorPaletteFile, IAveFile fontSchemeFile)
        {
            SPFile spColorFile = null;
            SPFile spFontFile = null;
            if (colorPaletteFile != null)
            {
                spColorFile = (colorPaletteFile as AveFile).File;
            }
            if (fontSchemeFile != null)
            {
                spFontFile = (fontSchemeFile as AveFile).File;
            }
            SPTheme sPTheme = SPTheme.Open(name, spColorFile, spFontFile);
            return new AveTheme(sPTheme);
        }

        public IAveTheme Open(string name, IAveFile colorPaletteFile, IAveFile fontSchemeFile, Uri backgroundImageUri)
        {
            SPFile spColorFile = null;
            SPFile spFontFile = null;
            if (colorPaletteFile != null)
            {
                spColorFile = (colorPaletteFile as AveFile).File;
            }
            if (fontSchemeFile != null)
            {
                spFontFile = (fontSchemeFile as AveFile).File;
            }
            SPTheme sPTheme = SPTheme.Open(name, spColorFile, spFontFile, backgroundImageUri);
            return new AveTheme(sPTheme);
        }

        public IAveTheme OpenAppliedTheme(IAveWeb web)
        {
            SPWeb spWeb = null;
            if (web != null)
            {
                spWeb = (web as AveWeb).Web;
            }
            SPTheme sPTheme = SPTheme.OpenAppliedTheme(spWeb);
            return new AveTheme(sPTheme);
        }

        public IAveTheme OpenFromXml(IAveFile spthemeXml)
        {
            SPFile spFile = null;
            if (spthemeXml != null)
            {
                spFile = (spthemeXml as AveFile).File;
            }
            SPTheme sPTheme = SPTheme.OpenFromXml(spFile);
            return new AveTheme(sPTheme);
        }

        public IAveTheme OpenFromXml(IAveFile spthemeXml, bool readPublishedVersion)
        {
            SPFile spFile = null;
            if (spthemeXml != null)
            {
                spFile = (spthemeXml as AveFile).File;
            }
            SPTheme sPTheme = SPTheme.OpenFromXml(spFile, readPublishedVersion);
            return new AveTheme(sPTheme);
        }

    }
}
