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
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AvePoint.Labs.AutoInstallation.Meetings.App.Installation.Base;
using AvePoint.Labs.AutoInstallation.Meetings.App.Installation.ViewModel.binding;

namespace AvePoint.Labs.AutoInstallation.Meetings.App.Installation.View
{
    /// <summary>
    /// Interaction logic for PageLicenseAgreement.xaml
    /// </summary>
    public partial class PageLicenseAgreement : Page
    {

        BaseDataContext baseFactory = BaseDataContext.GetInstance();
        private InstanceInstallationInfo instanceInstallationInfo = InstanceInstallationInfo.GetInstance();
        public static ResourceManager license = new ResourceManager("AvePoint.Labs.AutoInstallation.Meetings.App.Installation.I18N.AVELicense", Assembly.GetExecutingAssembly());
        public static CultureInfo culture = null;


        public PageLicenseAgreement()
        {
            InitializeComponent();
            instanceInstallationInfo.LicenseAgreement = license.GetString("EN_LicenseAgreement", culture);
            this.DataContext = instanceInstallationInfo;
        }

        private void Pagr_Loaded(object sender, RoutedEventArgs e)
        {
            GAAgreement.Focus();
        }

        private void GAAgreement_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox curr = sender as CheckBox;
            BindingExpression binding = curr.GetBindingExpression(CheckBox.IsCheckedProperty);
            binding.UpdateSource();
            baseFactory.NextButtonOperator.Command.OnCanExecuteChanged();
        }

    }
}
