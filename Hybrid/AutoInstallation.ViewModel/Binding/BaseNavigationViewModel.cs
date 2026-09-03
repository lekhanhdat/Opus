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


using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Media;
using AutoInstallation.Contract;
using AutoInstallation.Contract.Interface.Navigation;
using AutoInstallation.Contract.Navigation;
using AutoInstallationCommon.Utility;
using COMMONRESX = AutoInstallation.Records.App.Resources.Resource;

namespace AutoInstallation.ViewModel.Binding
{
    public class BaseNavigationViewModel : NotifyPropertyChanged, INavigationViewModel
    {
        private static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private int currentIndex = -1;
        private Page mainContent;

        private string title = string.Empty;
        private ImageSource titleImage;
        private Page wizardItem;
        private ObservableCollection<LogicWizardItem> wizards = new ObservableCollection<LogicWizardItem>();

        public int CurrentIndex
        {
            get
            {
                if (currentIndex >= Wizards.Count)
                    return wizards.Count - 1;
                return currentIndex;
            }
        }

        public LogicWizardItem CurrentItem
        {
            get
            {
                if (currentIndex < 0)
                    return wizards[0];
                if (currentIndex > wizards.Count - 1)
                    return wizards[0];
                return wizards[currentIndex];
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

        public Page MainContent
        {
            get { return mainContent; }
            set
            {
                mainContent = value;
                OnPropertyChanged("MainContent");
            }
        }

        public Page WizardItem
        {
            get { return wizardItem; }
            set
            {
                wizardItem = value;
                OnPropertyChanged("WizardItem");
            }
        }

        public void Next()
        {
            if (currentIndex < Wizards.Count - 1)
            {
                currentIndex++;
                MainContent = CurrentItem.Page;
                ChangeLeftSelect(currentIndex);
            }
        }

        public void Back()
        {
            if (currentIndex > 0)
            {
                currentIndex--;
                MainContent = CurrentItem.Page;
                ChangeLeftSelect(currentIndex);
            }
            else
            {
                currentIndex--;
                MainContent = null;
                ChangeLeftSelect(currentIndex);
            }
        }

        public ObservableCollection<LogicWizardItem> Wizards
        {
            get { return wizards; }
            set
            {
                wizards = value;
                OnPropertyChanged("Wizards");
            }
        }

        public string Title
        {
            get { return title; }
            set
            {
                title = value;
                OnPropertyChanged("Title");
            }
        }

        private void ChangeLeftSelect(int index)
        {
            for (var i = 0; i < Wizards.Count; i++)
                if (i == index)
                    Wizards[i].UnitState = WizardUnitState.Configuring;
                else if (i < index)
                    Wizards[i].UnitState = WizardUnitState.Configured;
                else
                    Wizards[i].UnitState = WizardUnitState.Waiting;
        }
    }
}