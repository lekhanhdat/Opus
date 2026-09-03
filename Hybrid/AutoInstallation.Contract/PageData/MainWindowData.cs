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


using System.Windows.Controls;
using System.Windows.Media;

namespace AutoInstallation.Contract.PageData
{
    public class MainWindowData : NotifyPropertyChanged
    {
        private Page fullScreenPage;
        private bool fullSreenFrameVisbility = true;
        private Page mainContentPage;
        private string title = string.Empty;

        /// <summary>
        ///     24*24
        /// </summary>
        private ImageSource titleImage;

        private bool wizardType = true;

        /// <summary>
        ///     true-导航模式，false-全屏模式
        /// </summary>
        public bool WizardType
        {
            get { return wizardType; }
            set
            {
                wizardType = value;
                OnPropertyChanged("WizardType");
            }
        }

        public ImageSource TitleImage
        {
            get { return titleImage; }
            set
            {
                titleImage = value;
                OnPropertyChanged("TitleImage");
            }
        }

        /// <summary>
        ///     .ico
        /// </summary>
        public ImageSource IConImage { get; set; }

        public string Title
        {
            get { return title; }
            set
            {
                title = value;
                OnPropertyChanged("Title");
            }
        }

        public Page FullScreenPage
        {
            get { return fullScreenPage; }
            set
            {
                fullScreenPage = value;
                OnPropertyChanged("FullScreenPage");
            }
        }

        public bool FullSreenFrameVisbility
        {
            get { return fullSreenFrameVisbility; }
            set
            {
                fullSreenFrameVisbility = value;
                OnPropertyChanged("FullSreenFrameVisbility");
            }
        }

        public Page MainContentPage
        {
            get { return mainContentPage; }
            set
            {
                mainContentPage = value;
                OnPropertyChanged("MainContentPage");
            }
        }
    }
}