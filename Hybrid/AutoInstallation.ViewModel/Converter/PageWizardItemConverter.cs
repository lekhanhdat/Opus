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
using System.Globalization;
using System.Windows.Data;
using AutoInstallation.Contract.DataBase;
using AutoInstallation.Contract.Navigation;

namespace AutoInstallation.ViewModel.Converter
{
    public class PageWizardItemConverter
    {
    }

    public class PageWizardItemOpacityConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double reValue = 1;

            switch ((WizardUnitState) value)
            {
                case WizardUnitState.Finished:
                    reValue = 0;
                    break;
                case WizardUnitState.Configured:
                    reValue = 0;
                    break;
                case WizardUnitState.Configuring:
                    reValue = 1;
                    break;
                case WizardUnitState.Waiting:
                    reValue = 0;
                    break;
            }

            return reValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion
    }

    public class PageWizardItemUnitStateIconConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var reValue =
                "/AutoInstallation.Common.GUI;component/ItemControl/PageWizardItem/Images/wi_Unconfigured_12x12.png";

            switch ((WizardUnitState) value)
            {
                case WizardUnitState.Finished:
                    reValue = "";
                    break;
                case WizardUnitState.Configured:
                    reValue =
                        "/AutoInstallation.Common.GUI;component/ItemControl/PageWizardItem/Images/wi_Configured_12x12.png";
                    break;
                case WizardUnitState.Configuring:
                    reValue =
                        "/AutoInstallation.Common.GUI;component/ItemControl/PageWizardItem/Images/wi_Configuring_12x12.png";
                    break;
                case WizardUnitState.Waiting:
                    reValue = "";
                    break;
            }

            return reValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion
    }

    public class PageWizardItemUnitTypeLocationConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var reValue = "0,5,0,5";

            switch ((WizardUnitType) value)
            {
                case WizardUnitType.FirstLevel:
                    reValue = "0,5,0,5";
                    break;
                case WizardUnitType.SecondLevel:
                    reValue = "20,5,0,5";
                    break;
                case WizardUnitType.ThirdLevel:
                    reValue = "40,5,0,5";
                    break;
            }

            return reValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion
    }

    public class AuthenticationToEnableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var reValue = false;

            switch ((Authentication) value)
            {
                case Authentication.WindowsAuthentication:
                    reValue = false;
                    break;
                case Authentication.SQLAuthentication:
                    reValue = true;
                    break;
            }

            return reValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}