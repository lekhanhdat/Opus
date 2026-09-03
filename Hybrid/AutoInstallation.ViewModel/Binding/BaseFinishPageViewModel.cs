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


using System.Windows;
using System.Windows.Media;
using AutoInstallation.Contract;
using AutoInstallation.Contract.Interface.FinishPage;
using COMMONRESX = AutoInstallation.Records.App.Resources.Resource;

namespace AutoInstallation.ViewModel.Binding
{
    public class BaseFinishPageViewModel : NotifyPropertyChanged, IFinishPageViewModel
    {
        private string reportLocation = string.Empty;
        private Visibility reportVis = Visibility.Collapsed;
        private string title = string.Empty;

        public string Title
        {
            get { return title; }
            set
            {
                title = value;
                OnPropertyChanged("Title");
            }
        }

        public string TopTitle { get; set; } = string.Empty;

        public ImageSource WarterMarkImage { get; set; }
        public string Message { get; set; }

        public string ReportLocation
        {
            get { return reportLocation; }
            set
            {
                reportLocation = value;
                OnPropertyChanged("ReportLocation");
            }
        }

        public Visibility ReportVis
        {
            get { return reportVis; }
            set
            {
                reportVis = value;
                OnPropertyChanged("ReportVis");
            }
        }

        public string ReportLocationTitle { get; }/* = COMMONRESX.COMMON_GUI_TITLE_REPORTLOCATION + ":";*/

        public string ReportFilePath { get; set; }
    }
}