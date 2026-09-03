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

namespace AvePoint.ObjectModel.Server13
{
    public class AveFont : IAveFont
    {
        private SPFont mFont;
        private Dictionary<string, IAveThemeFont> mFontSlots;

        public AveFont()
        {
        }

        public AveFont(SPFont font)
        {
            if (font == null)
            {
                throw new ArgumentNullException("font");
            }
            mFont = font;
        }

        internal SPFont Font
        {
            get
            {
                return mFont;
            }
        }

        public Dictionary<string, IAveThemeFont> FontSlots
        {
            get
            {
                if (mFontSlots == null)
                {
                    mFontSlots = new Dictionary<string, IAveThemeFont>();
                    foreach (KeyValuePair<string, ThemeFont> item in mFont.FontSlots)
                    {
                        mFontSlots.Add(item.Key, new AveThemeFont(item.Value));
                    }
                }
                return mFontSlots;
            }
        }

        public string Name
        {
            get { return mFont.Name; }
        }

        public string PreviewSlot1
        {
            get { return mFont.PreviewSlot1; }
        }

        public string PreviewSlot2
        {
            get { return mFont.PreviewSlot2; }
        }

        public string ServerRelativeUrl
        {
            get { return mFont.ServerRelativeUrl; }
        }

        public IAveThemeFont GetFont(string slot)
        {
            return new AveThemeFont(mFont.GetFont(slot));
        }

        public System.Collections.ObjectModel.ReadOnlyCollection<IAveFont> GetFontSchemesFromFolder(IAveSite site, string strThemeFolder)
        {
            List<IAveFont> schemes = new List<IAveFont>();
            foreach (SPFont font in SPFont.GetFontSchemesFromFolder((site as AveSite).Site, strThemeFolder))
            {
                schemes.Add(new AveFont(font));
            }
            return new System.Collections.ObjectModel.ReadOnlyCollection<IAveFont>(schemes);
        }

        public IAveFont Open(IAveFile file)
        {
            SPFile spFile = null;
            if (file != null)
            {
                spFile = (file as AveFile).File;
            }
            return new AveFont(SPFont.Open(spFile));
        }

        public IAveFont Open(IAveFile file, bool readPublishedVersion)
        {
            SPFile spFile = null;
            if (file != null)
            {
                spFile = (file as AveFile).File;
            }
            return new AveFont(SPFont.Open(spFile, readPublishedVersion));
        }
    }
}
