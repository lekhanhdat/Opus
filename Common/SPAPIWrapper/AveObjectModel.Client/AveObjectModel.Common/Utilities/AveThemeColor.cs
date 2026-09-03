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
using System.Globalization;
using System.Diagnostics.CodeAnalysis;

namespace AvePoint.ObjectModel.Common
{
    public class AveThemeColor : IAveThemeColor
    {
        private string mDefaultColor;

        public AveThemeColor(string value)
        {
            mDefaultColor = GetDefaultColorFromARGB("#" + value);
        }

        public string DefaultColor
        {
            get
            {
                return mDefaultColor;
            }
            set
            {
                mDefaultColor = value;
            }
        }

        public System.Collections.ObjectModel.ReadOnlyCollection<string> Shades
        {
            get { throw new NotImplementedException(); }
        }

        public string GetScreenNameForColor(string color)
        {
            throw new NotImplementedException();
        }

        public System.Collections.ObjectModel.ReadOnlyCollection<string> GetShadesForColor(string color)
        {
            throw new NotImplementedException();
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "argd is a part of keys")]
        private string GetDefaultColorFromARGB(string argb)
        {
            if (string.IsNullOrEmpty(argb))
            {
                throw new ArgumentNullException("argb", "Invalid color string. The value specified must be in the form AARRGGBB, RRGGBB, or RGB");
            }
            if (argb[0] != '#')
            {
                throw new ArgumentException("Invalid color string. The value specified must be in the form AARRGGBB, RRGGBB, or RGB", "argb");
            }
            byte result = 0xff;
            byte num2 = 0;
            byte num3 = 0;
            byte num4 = 0;
            if (argb.Length == 9)
            {
                string[] strArray = new string[] { argb.Substring(1, 2), argb.Substring(3, 2), argb.Substring(5, 2), argb.Substring(7, 2) };
                if ((!byte.TryParse(strArray[0], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out result) || !byte.TryParse(strArray[1], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out num2)) || (!byte.TryParse(strArray[2], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out num3) || !byte.TryParse(strArray[3], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out num4)))
                {
                    throw new ArgumentException("Invalid color string. The value specified must be in the form AARRGGBB, RRGGBB, or RGB", "argb");
                }
            }
            else if (argb.Length == 7)
            {
                string[] strArray2 = new string[] { argb.Substring(1, 2), argb.Substring(3, 2), argb.Substring(5, 2) };
                if ((!byte.TryParse(strArray2[0], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out num2) || !byte.TryParse(strArray2[1], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out num3)) || !byte.TryParse(strArray2[2], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out num4))
                {
                    throw new ArgumentException("Invalid color string. The value specified must be in the form AARRGGBB, RRGGBB, or RGB", "argb");
                }
            }
            else
            {
                if (argb.Length != 4)
                {
                    throw new ArgumentException("Invalid color string. The value specified must be in the form AARRGGBB, RRGGBB, or RGB", "argb");
                }
                string[] strArray3 = new string[] { string.Empty + argb[1] + argb[1], string.Empty + argb[2] + argb[2], string.Empty + argb[3] + argb[3] };
                if ((!byte.TryParse(strArray3[0], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out num2) || !byte.TryParse(strArray3[1], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out num3)) || !byte.TryParse(strArray3[2], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out num4))
                {
                    throw new ArgumentException("Invalid color string. The value specified must be in the form AARRGGBB, RRGGBB, or RGB", "argb");
                }
            }
            byte[] values = new byte[] { result, num2, num3, num4 };
            return BitConverter.ToString(values);
        }
    }
}
