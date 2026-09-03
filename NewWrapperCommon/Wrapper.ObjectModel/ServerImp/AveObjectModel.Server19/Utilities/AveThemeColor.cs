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
using Microsoft.SharePoint.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;

namespace AvePoint.ObjectModel.Server19
{
    public class AveThemeColor : IAveThemeColor
    {
        private ThemeColor mThemeColor;

        public AveThemeColor(ThemeColor color)
        {
            if (color == null)
            {
                throw new ArgumentNullException("color");
            }
            mThemeColor = color;
        }

        internal ThemeColor ThemeColor
        {
            get
            {
                return mThemeColor;
            }
        }

        public string DefaultColor
        {
            get
            {
                return ColorConvertToString(mThemeColor.DefaultColor);
            }
            set
            {
                StringConvertToColor(value);
            }
        }

        private ReadOnlyCollection<string> mShades;

        public ReadOnlyCollection<string> Shades
        {
            get
            {
                if (mShades == null)
                {
                    List<string> shadeList = new List<string>();
                    foreach (Color color in mThemeColor.Shades)
                    {
                        shadeList.Add(ColorConvertToString(color));
                    }
                    mShades = new ReadOnlyCollection<string>(shadeList);
                }
                return mShades;
            }
        }

       [SuppressMessage("Microsoft.Globalization", "CA1304:SpecifyCultureInfo", MessageId = "Microsoft.SharePoint.Utilities.ThemeColor.GetScreenNameForColor(System.Drawing.Color)")]
       public string GetScreenNameForColor(string color)
        {
            return ThemeColor.GetScreenNameForColor(StringConvertToColor(color));
        }

        public ReadOnlyCollection<string> GetShadesForColor(string color)
        {
            List<string> shadeList = new List<string>();
            foreach (Color c in ThemeColor.GetShadesForColor(StringConvertToColor(color)))
            {
                shadeList.Add(ColorConvertToString(c));
            }
            return new ReadOnlyCollection<string>(shadeList);
        }

        private string ColorConvertToString(Color color)
        {
            byte[] values = new byte[] { color.A, color.R, color.G, color.B };
            return BitConverter.ToString(values);
        }

        private Color StringConvertToColor(string color)
        {
            string[] colorARGB = color.Split(new char[] { '-' });
            if (colorARGB.Count() != 4)
            {
                return Color.Empty;
            }
            return Color.FromArgb(Convert.ToInt32(colorARGB[0], 16), Convert.ToInt32(colorARGB[1], 16), Convert.ToInt32(colorARGB[2], 16), Convert.ToInt32(colorARGB[3], 16));
        }
    }
}
