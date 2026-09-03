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
using System.Windows;
using System.Windows.Data;
using AutoInstallation.Contract;

namespace AutoInstallationCommon.Utility
{
    public class InstallModelConverter : IValueConverter
    {
        #region IValueConverter Members

        /// <summary>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter">
        ///     0.install checkbox
        ///     1.uninstall checkbox
        ///     2.upgrade checkbox
        /// </param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var reValue = true;

            switch ((InstallationModel) value)
            {
                case InstallationModel.Install:
                    if (int.Parse(parameter.ToString()) != 0) reValue = false;
                    break;
                case InstallationModel.Uninstall:
                    if (int.Parse(parameter.ToString()) != 1) reValue = false;
                    break;
                case InstallationModel.Upgrade:
                    if (int.Parse(parameter.ToString()) != 2) reValue = false;
                    break;
                case InstallationModel.GeneratePackage:
                    if (int.Parse(parameter.ToString()) != 5) reValue = false;
                    break;
            }

            return reValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var reValue = InstallationModel.Install;
            if ((bool) value) reValue = (InstallationModel) int.Parse(parameter.ToString());
            return reValue;
        }

        #endregion
    }

    public class InstallModelToVisibilityConverter : IValueConverter
    {
        #region IValueConverter Members

        /// <summary>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter">
        ///     0.install checkbox
        ///     1.uninstall checkbox
        ///     2.upgrade checkbox
        /// </param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var reValue = Visibility.Visible;

            switch ((InstallationModel) value)
            {
                case InstallationModel.Install:
                    if (int.Parse(parameter.ToString()) != 0) reValue = Visibility.Collapsed;
                    break;
                case InstallationModel.Uninstall:
                    if (int.Parse(parameter.ToString()) != 1) reValue = Visibility.Collapsed;
                    break;
                case InstallationModel.Upgrade:
                    if (int.Parse(parameter.ToString()) != 2) reValue = Visibility.Collapsed;
                    break;
                case InstallationModel.GeneratePackage:
                    if (int.Parse(parameter.ToString()) != 5) reValue = Visibility.Collapsed;
                    break;
            }

            return reValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //InstallationModel reValue = InstallationModel.Install;
            //if ((bool)value)
            //{
            //    reValue = (InstallationModel)(int.Parse(parameter.ToString()));
            //}
            //return reValue;
            return null;
        }

        #endregion
    }
}