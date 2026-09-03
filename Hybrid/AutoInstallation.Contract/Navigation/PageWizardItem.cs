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


using System.ComponentModel;

namespace AutoInstallation.Contract.Navigation
{
    public class PageWizardItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Content

        private string content;

        public string Content
        {
            get { return content; }
            set
            {
                content = value;
                OnPropertyChanged("Content");
            }
        }

        #endregion

        #region isCurrent

        private bool isCurrent;

        public bool IsCurrent
        {
            get { return isCurrent; }
            set
            {
                isCurrent = value;
                OnPropertyChanged("IsCurrent");
            }
        }

        #endregion

        #region isVisibility

        private bool isVisibility = true;

        public bool IsVisibility
        {
            get { return isVisibility; }
            set
            {
                isVisibility = value;
                OnPropertyChanged("IsVisibility");
            }
        }

        #endregion

        #region isChecked

        private bool isChecked;

        public bool IsChecked
        {
            get { return isChecked; }
            set
            {
                isChecked = value;
                OnPropertyChanged("IsChecked");
            }
        }

        #endregion


        #region isEnabled

        private bool isEnabled;

        public bool IsEnabled
        {
            get { return isEnabled; }
            set
            {
                isEnabled = value;
                OnPropertyChanged("IsEnabled");
            }
        }

        #endregion

        #region IsConfigured

        private bool isConfigured;

        public bool IsConfigured
        {
            get { return isConfigured; }
            set
            {
                isConfigured = value;
                OnPropertyChanged("IsConfigured");
            }
        }

        #endregion

        #region IsFather

        private bool isFather;

        public bool IsFather
        {
            get { return isFather; }
            set
            {
                isFather = value;
                OnPropertyChanged("IsFather");
            }
        }

        #endregion

        #region IsGrandfather

        private bool isGrandfather;

        public bool IsGrandfather
        {
            get { return isGrandfather; }
            set
            {
                isGrandfather = value;
                OnPropertyChanged("IsGrandfather");
            }
        }

        #endregion

        #region UnitState

        private WizardUnitState unitState;

        public WizardUnitState UnitState
        {
            get { return unitState; }
            set
            {
                unitState = value;
                OnPropertyChanged("UnitState");
            }
        }

        #endregion

        #region UnitType

        private WizardUnitType unitType;

        public WizardUnitType UnitType
        {
            get { return unitType; }
            set
            {
                unitType = value;
                OnPropertyChanged("UnitType");
            }
        }

        #endregion
    }
}