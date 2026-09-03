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
    using System.Windows.Data;
    using static AvePoint.Deployment.CommonGUI.PageWizardItem;

    #endregion

    public class PageWizardItemConverter
    {
    }

    public class PageWizardItemUnitStateIconConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            String reValue =
                "/AvePoint.CallAssist.CommonDeploymentGUI;component/ItemControl/PageWizardItem/Images/wi_Unconfigured_16x16.png";

            switch((WizardUnitState) value)
            {
                case WizardUnitState.Finished:
                    reValue = "";
                    break;
                case WizardUnitState.Configured:
                    reValue = "/AvePoint.CallAssist.CommonDeploymentGUI;component/ItemControl/PageWizardItem/Images/wi_Configured_16x16.png";
                    break;
                case WizardUnitState.Configuring:
                    reValue =
                        "/AvePoint.CallAssist.CommonDeploymentGUI;component/ItemControl/PageWizardItem/Images/wi_Configuring_16x16.png";
                    break;
                case WizardUnitState.Waiting:
                    reValue =
                        " ";
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

    public class PageWizardItemUnitTypeLocationConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            String reValue = "0,5,0,5";

            switch((WizardUnitType) value)
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

        public object ConvertBack(object value,
                                  Type targetType,
                                  object parameter,
                                  System.Globalization.CultureInfo culture)
        {
            return null;
        }

        #endregion
    }
}