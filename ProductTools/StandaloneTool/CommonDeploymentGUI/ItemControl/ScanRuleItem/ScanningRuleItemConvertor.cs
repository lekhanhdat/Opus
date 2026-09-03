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

namespace AvePoint.Deployment.CommonGUI
{
    #region ---namespace---

    using System;
    using System.Windows.Media;
    using System.Windows.Data;

    #endregion

    public class ScanningRuleItemConvertor
    {
    }

    public class ScanningRuleItemCheckStateIconMarkConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            String reValue = "/AvePoint.CallAssist.CommonDeploymentGUI;component/ItemControl/ScanRuleItem/Images/sii_waiting_16x16.png";
            switch((SystemInfoCheckStatus) value)
            {
                case SystemInfoCheckStatus.Mismatch:
                    reValue = "/AvePoint.CallAssist.CommonDeploymentGUI;component/ItemControl/ScanRuleItem/Images/sii_mismatch_16x16.png";
                    break;
                case SystemInfoCheckStatus.Warning:
                    reValue = "/AvePoint.CallAssist.CommonDeploymentGUI;component/ItemControl/ScanRuleItem/Images/sii_warning_16x16.png";
                    break;
                case SystemInfoCheckStatus.Passed:
                    reValue = "/AvePoint.CallAssist.CommonDeploymentGUI;component/ItemControl/ScanRuleItem/Images/sii_passed_16x16.png";
                    break;
                default:
                    reValue = "/AvePoint.CallAssist.CommonDeploymentGUI;component/ItemControl/ScanRuleItem/Images/sii_waiting_16x16.png";
                    break;
            }
            return reValue;
        }

        public object ConvertBack(object value,
                                  Type targetType,
                                  object parameter,
                                  System.Globalization.CultureInfo culture)
        {
            return null;
        }

        #endregion
    }

    public class ScanningRuleItemCheckStateStatusStringConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            String reValue = "Waiting";

            switch((SystemInfoCheckStatus) value)
            {
                case SystemInfoCheckStatus.Mismatch:
                    reValue = "Failed";
                    break;
                case SystemInfoCheckStatus.Warning:
                    reValue = "Warning";
                    break;
                case SystemInfoCheckStatus.Passed:
                    reValue = "Passed";
                    break;
                default:
                    reValue = "Waiting";
                    break;
            }
            return reValue;
        }

        public object ConvertBack(object value,
                                  Type targetType,
                                  object parameter,
                                  System.Globalization.CultureInfo culture)
        {
            return null;
        }

        #endregion
    }

    public class ScanningRuleItemCheckStateForegroundConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            SolidColorBrush reValue = new SolidColorBrush(GetColorFromString("#FF333333"));

            switch((SystemInfoCheckStatus) value)
            {
                case SystemInfoCheckStatus.Mismatch:
                    reValue = new SolidColorBrush(GetColorFromString("#FFFF3333"));
                    break;
                case SystemInfoCheckStatus.Warning:
                    reValue = new SolidColorBrush(GetColorFromString("#FFFF9900"));
                    break;
                case SystemInfoCheckStatus.Passed:
                    reValue = new SolidColorBrush(GetColorFromString("#FF22AA11"));
                    break;
                default: //waiting
                    reValue = new SolidColorBrush(GetColorFromString("#FF333333"));
                    break;
            }
            return reValue;
        }

        public object ConvertBack(object value,
                                  Type targetType,
                                  object parameter,
                                  System.Globalization.CultureInfo culture)
        {
            return null;
        }

        #endregion

        /// <summary>
        /// convert ARGB string to Color.
        /// </summary>
        /// <param name="argb"></param>
        /// <returns></returns>
        public static Color GetColorFromString(string argb)
        {
            if(!argb.Length.Equals(9) && !argb.Length.Equals(7))
            {
                return Colors.White;
            }
            Color ret = new Color();
            byte a = 255, r, g, b;
            bool hasAlpha = argb.Length.Equals(9) ? true : false;
            if(hasAlpha)
            {
                a = (byte) (System.Convert.ToUInt32(argb.Substring(1, 2), 16));
                r = (byte) (System.Convert.ToUInt32(argb.Substring(3, 2), 16));
                g = (byte) (System.Convert.ToUInt32(argb.Substring(5, 2), 16));
                b = (byte) (System.Convert.ToUInt32(argb.Substring(7, 2), 16));
            }
            else
            {
                r = (byte) (System.Convert.ToUInt32(argb.Substring(1, 2), 16));
                g = (byte) (System.Convert.ToUInt32(argb.Substring(3, 2), 16));
                b = (byte) (System.Convert.ToUInt32(argb.Substring(5, 2), 16));
            }
            ret = Color.FromArgb(a, r, g, b);
            return ret;
        }
    }
}