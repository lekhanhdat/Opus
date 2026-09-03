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



using System.Collections.Generic;
using AvePoint.Wrapper.Common;
using System.Collections.ObjectModel;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint;
using System;

namespace AvePoint.ObjectModel.Server19
{
    class AveThmxTheme : IAveThmxTheme
    {
        private AveSite mSite;
        private ThmxTheme mThmxTheme;
        private AveFile mFile;

        public AveThmxTheme(IAveSite site)
        {
            mSite = site as AveSite;
        }

        public AveThmxTheme(ThmxTheme thmxTheme)
        {
            mThmxTheme = thmxTheme;
        }

        public ReadOnlyCollection<IAveThmxTheme> GetManagedThemes(IAveSite site)
        {
            List<IAveThmxTheme> managedThemes = new List<IAveThmxTheme>();
            foreach (ThmxTheme thmxTheme in ThmxTheme.GetManagedThemes((site as AveSite).Site))
            {
                if (thmxTheme != null)
                {
                    managedThemes.Add(new AveThmxTheme(thmxTheme));
                }
                else
                {
                    managedThemes.Add(null);
                }
            }
            return managedThemes.AsReadOnly();
        }

        public string GetThemeUrlForWeb(IAveWeb web)
        {
            return ThmxTheme.GetThemeUrlForWeb((web as AveWeb).Web);
        }

        public IAveThmxTheme Open(IAveSite site, string url)
        {
            if (site == null)
            {
                throw new ArgumentNullException("site");
            }
            using (IAveWeb web = site.OpenWeb(url, false))
            {
                mFile = web.GetFile(url) as AveFile;
                ThmxTheme thmxTheme = ThmxTheme.Open((site as AveSite).Site, url);
                if (thmxTheme == null)
                {
                    return null;
                }
                return new AveThmxTheme(thmxTheme);
            }
        }

        public void RemoveThemeFromWeb(IAveWeb web, bool deleteFiles)
        {
            ThmxTheme.RemoveThemeFromWeb((web as AveWeb).Web, deleteFiles);
        }

        public string Name
        {
            get
            {
                return mThmxTheme.Name;
            }
            set
            {
                mThmxTheme.Name = value;
            }
        }

        public void Close()
        {
            mThmxTheme.Close();
        }

        public void ApplyTo(IAveWeb web, bool shareGenerated)
        {
            mThmxTheme.ApplyTo((web as AveWeb).Web, shareGenerated);
        }

        public void Dispose()
        {
            //throw new System.NotImplementedException();
            ((IDisposable)mThmxTheme).Dispose();
        }

        public void SetThemeUrlForWeb(IAveWeb web, string themeUrl)
        {
            ThmxTheme.SetThemeUrlForWeb((web as AveWeb).Web, themeUrl);
        }

        public string ServerRelativeUrl
        {
            get
            {
                return mThmxTheme.ServerRelativeUrl;
            }
        }

        public IAveFile File
        {
            get
            {
                if (mFile == null)
                {
                    SPFile file = mThmxTheme.File;
                    if (file != null)
                    {
                        mFile = new AveFile(new AveWeb(mSite, file.Web), file);
                    }
                }
                return mFile;
            }
        }
    }
}
