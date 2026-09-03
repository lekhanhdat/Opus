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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace AvePoint.ObjectModel.Server16
{
    public class AveColor : IAveColor
    {
        private SPColor mColor;

        private Dictionary<string, IAveThemeColor> mColors;

        public AveColor()
        {
        }

        public AveColor(SPColor color)
        {
            if (color == null)
            {
                throw new ArgumentNullException("color");
            }
            mColor = color;
        }

        internal SPColor Color
        {
            get
            {
                return mColor;
            }
        }

        public string AccessibleDescription
        {
            get
            {
                if (mColor == null)
                {
                    return null;
                }
                return mColor.AccessibleDescription;
            }
        }

        public Dictionary<string, IAveThemeColor> Colors
        {
            get
            {
                if (mColor == null)
                {
                    return null;
                }
                if (mColors == null)
                {
                    mColors = new Dictionary<string, IAveThemeColor>();
                    foreach (KeyValuePair<string, ThemeColor> color in mColor.Colors)
                    {
                        mColors.Add(color.Key, new AveThemeColor(color.Value));
                    }
                }
                return mColors;
            }
        }

        public string PreviewSlot1
        {
            get
            {
                if (mColor == null)
                {
                    return null;
                }
                return mColor.PreviewSlot1;
            }
        }

        public string PreviewSlot2
        {
            get
            {
                if (mColor == null)
                {
                    return null;
                }
                return mColor.PreviewSlot2;
            }
        }

        public string PreviewSlot3
        {
            get
            {
                if (mColor == null)
                {
                    return null;
                }
                return mColor.PreviewSlot3;
            }
        }

        public string ServerRelativeUrl
        {
            get
            {
                if (mColor == null)
                {
                    return null;
                }
                return mColor.ServerRelativeUrl;
            }
        }

        public ReadOnlyCollection<IAveColor> GetColorPalettesFromFolder(IAveSite site, string strThemeFolder, bool recursive)
        {
            SPSite spSite = null;
            if (site != null)
            {
                spSite = (site as AveSite).Site;
            }
            List<IAveColor> result = new List<IAveColor>();
            ReadOnlyCollection<SPColor> colors = SPColor.GetColorPalettesFromFolder(spSite, strThemeFolder, recursive);
            foreach (SPColor color in colors)
            {
                result.Add(new AveColor(color));
            }
            return new ReadOnlyCollection<IAveColor>(result);
        }

        public IAveColor Open(IAveFile file)
        {
            SPFile spFile = null;
            if (file != null)
            {
                spFile = (file as AveFile).File;
            }
            return new AveColor(SPColor.Open(spFile));
        }

        public IAveColor Open(IAveFile file, bool readPublishedVersion)
        {
            SPFile spFile = null;
            if (file != null)
            {
                spFile = (file as AveFile).File;
            }
            return new AveColor(SPColor.Open((file as AveFile).File, readPublishedVersion));
        }
    }
}
