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


using System.Threading;
using System.Windows;
using System.Windows.Media;
using AutoInstallation.Contract;
using AutoInstallation.Contract.Interface;

namespace AutoInstallation.ViewModel.Binding
{
    public class BaseInstallProgressViewModel : NotifyPropertyChanged, IInstallProgressViewModel
    {
        private int processValue;
        private Visibility subProgressVis = Visibility.Collapsed;
        private string subTitle = string.Empty;
        public string Title { get; set; }

        public string SubTitle
        {
            get { return subTitle; }
            set
            {
                subTitle = value;
                OnPropertyChanged("SubTitle");
            }
        }

        public Visibility SubProgressVis
        {
            get { return subProgressVis; }
            set
            {
                subProgressVis = value;
                OnPropertyChanged("SubProgressVis");
            }
        }

        public int ProcessValue
        {
            get { return processValue; }
            set
            {
                processValue = value;
                OnPropertyChanged("ProcessValue");
            }
        }

        public string ProcessBarCount { get; set; }
        public ImageSource WarterMarkImage { get; set; }

        /// <summary>
        ///     刷新安装进度条
        /// </summary>
        public void RefreshInstallProgressBar(int s, int e)
        {
            for (var i = s; i <= e; i++)
            {
                Thread.Sleep(50);
                ProcessValue = i;
            }
        }
    }
}