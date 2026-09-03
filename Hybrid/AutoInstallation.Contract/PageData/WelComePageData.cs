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
using COMMONRESX = AutoInstallation.Records.App.Resources.Resource;

namespace AutoInstallation.Contract.PageData
{
    public class WelComePageData : NotifyPropertyChanged
    {
        /// <summary>
        ///     105*120
        /// </summary>
        private ImageSource contentImage;

        private InstallationModel installModel;
        private string installTag = string.Empty;
        private string installText;

        private Visibility isShowUpgrade = Visibility.Visible;

        private Visibility isShowO365 = Visibility.Visible;

        private Visibility isShowInstall = Visibility.Visible;

        /// <summary>
        ///     105*25
        /// </summary>
        private ImageSource titleImage;

        private string uninstallTag = string.Empty;
        private string generatePackageTag = string.Empty;
        private string upgradeTag = string.Empty;
        private string welcomeTag = string.Empty;

        //string ErrorMessage { get; set; }

        //Visibility ErrorVis { get; set; }
        private Visibility welcomeTagVis;

        private string welcomeText = string.Empty;

        public string UpgradeText { get; set; }/* = COMMONRESX.COMMON_TEXT_UPGRADE;*/

        public string GeneratePackageText { get; set; } /*= COMMONRESX.COMMON_TEXT_GeneratePackageText;*/

        public string InstallText
        {
            get { return installText; }
            set
            {
                installText = value;
                OnPropertyChanged("InstallText");
            }
        }

        public string GeneratePackageTag
        {
            get { return generatePackageTag; }
            set
            {
                generatePackageTag = value;
                OnPropertyChanged("GeneratePackageTag");
            }
        }

        public string UpgradeTag
        {
            get { return upgradeTag; }
            set
            {
                upgradeTag = value;
                OnPropertyChanged("UpgradeTag");
            }
        }

        public string InstallTag
        {
            get { return installTag; }
            set
            {
                installTag = value;
                OnPropertyChanged("InstallTag");
            }
        }

        public string UninstallTag
        {
            get { return uninstallTag; }
            set
            {
                uninstallTag = value;
                OnPropertyChanged("UninstallTag");
            }
        }

        public Visibility IsShowO365
        {
            get { return isShowO365; }
            set
            {
                isShowO365 = value;
                OnPropertyChanged("IsShowO365");
            }
        }

        public Visibility IsShowInstall
        {
            get { return isShowInstall; }
            set
            {
                isShowInstall = value;
                OnPropertyChanged("IsShowInstall");
            }
        }

        public Visibility IsShowUpgrade
        {
            get { return isShowUpgrade; }
            set
            {
                isShowUpgrade = value;
                OnPropertyChanged("IsShowUpgrade");
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

        public ImageSource ContentImage
        {
            get { return contentImage; }
            set
            {
                contentImage = value;
                OnPropertyChanged("ContentImage");
            }
        }

        public InstallationModel InstallMode
        {
            get { return installModel; }
            set
            {
                installModel = value;
                OnPropertyChanged("InstallMode");
            }
        }

        public string WelcomeText
        {
            get { return welcomeText; }
            set
            {
                welcomeText = value;
                OnPropertyChanged("WelcomeText");
            }
        }

        public string WelcomeTag
        {
            get { return welcomeTag; }
            set
            {
                welcomeTag = value;
                OnPropertyChanged("WelcomeTag");
            }
        }

        public Visibility WelcomeTagVis
        {
            get { return welcomeTagVis; }
            set
            {
                welcomeTagVis = value;
                OnPropertyChanged("WelcomeTagVis");
            }
        }
    }
}