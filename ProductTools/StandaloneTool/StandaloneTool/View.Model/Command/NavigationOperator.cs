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
using AvePoint.RA.CommonUtil;
using DataExportCore;
using StandaloneTool.Model;
using StandaloneTool.Model.Common;
using StandaloneTool.View.Model.Binding;
using static AvePoint.Deployment.CommonGUI.PageWizardItem;

namespace StandaloneTool.View.Model.Command
{
    public class NavigationOperator : BaseINotifyPropertyChanged
    {
        private readonly RALogger logger = RALogger.GetInstance(typeof(NavigationOperator));
        #region Properties

        private int currentID = -1;
        public int CurrentID
        {
            get { return currentID; }
            set { currentID = value; }
        }

        private Uri _topFrameSource;
        public Uri TopFrameSource
        {
            get { return _topFrameSource; }
            set
            {
                _topFrameSource = value;
                OnPropertyChanged("TopFrameSource");
            }
        }

        private Uri _coverFrameSource;
        public Uri CoverFrameSource
        {
            get { return _coverFrameSource; }
            set
            {
                _coverFrameSource = value;
                OnPropertyChanged("CoverFrameSource");
            }
        }

        private Uri _hostFrameSource;
        public Uri HostFrameSource
        {
            get { return _hostFrameSource; }
            set
            {
                _hostFrameSource = value;
                OnPropertyChanged("HostFrameSource");
            }
        }

        private Uri _leftFrameSource;
        public Uri LeftFrameSource
        {
            get { return _leftFrameSource; }
            set
            {
                _leftFrameSource = value;
                OnPropertyChanged("LeftFrameSource");
            }
        }

        #endregion

        public BaseDataContext baseDataContext = BaseDataContext.Instance;

        public PageFeatures CurrentPage = PageFeatures.WelcomePage;

        public NavigationOperator()
        {
            CoverFrameSource = new Uri("/View/SelectionPage.xaml", UriKind.Relative);
            _hostFrameSource = new Uri("/View/SelectionPage.xaml", UriKind.Relative);
            _leftFrameSource = new Uri("/View/ImportEncryptionKeyPage.xaml", UriKind.Relative);
            _topFrameSource = new Uri("WizardPage.xaml", UriKind.Relative);
        }

        public void SetCurrentPage(PageOperation operation)
        {
            try
            {
                if (operation == PageOperation.Next)
                {
                    SetNextPageHandler();
                }
                if (operation == PageOperation.Back)
                {
                    SetBackPageHandler();
                }
            }
            catch (Exception ex)
            {
                logger.Warn($"An error occurred while set current page with {PageOperation.Next} operation, current page [{CurrentPage}]: {ex}");
            }
        }

        private void SetBackPageHandler()
        {
            #region BACK Behavior
            int prevID = CurrentID;

            if (prevID == 3 && !GlobalInfo.IsUsingAveStorage) //Export location page
            {
                BackToPage(PageFeatures.RecoveryPage);
                return;
            }

            CurrentID--;

            if (CurrentID < 0)
            {
                baseDataContext.ModelCommonInfo.IsCover = false;
                CurrentPage = PageFeatures.WelcomePage;
                baseDataContext.NavigationOperator.CoverFrameSource = new Uri("/View/SelectionPage.xaml", UriKind.Relative);
                baseDataContext.NavigationOperator.HostFrameSource = new Uri("/View/SelectionPage.xaml", UriKind.Relative);
                baseDataContext.NavigationOperator.LeftFrameSource = new Uri("SelectionPage.xaml", UriKind.Relative);
                baseDataContext.NavigationOperator.TopFrameSource = new Uri("WizardPage.xaml", UriKind.Relative);
                return;
            }

            baseDataContext.ModelCommonInfo.IsCover = true;
            while (!WizardUnitDataInfo.GetInstance().WizardCollection[CurrentID].IsEnabled)
            {
                CurrentID--;
                if (CurrentID < 0)
                {
                    break;
                }
            }

            if (CurrentID > -1)
            {
                LeftFrameSource = new Uri(WizardUnitDataInfo.GetInstance().WizardCollection[CurrentID].PageLocation, UriKind.Relative);
                CurrentPage = WizardUnitDataInfo.GetInstance().WizardCollection[CurrentID].PageFeatures;
                WizardUnitDataInfo.GetInstance().WizardCollection[CurrentID].UnitState = WizardUnitState.Configuring;
                WizardUnitDataInfo.GetInstance().WizardCollection[CurrentID].WizardOperator.Command.OnCanExecuteChanged();
                if (prevID < WizardUnitDataInfo.GetInstance().WizardCollection.Count)
                {
                    if (WizardUnitDataInfo.GetInstance().WizardCollection[prevID].IsConfigured)
                    {
                        WizardUnitDataInfo.GetInstance().WizardCollection[prevID].UnitState = WizardUnitState.Configured;
                    }
                    else
                    {
                        WizardUnitDataInfo.GetInstance().WizardCollection[prevID].UnitState = WizardUnitState.Waiting;
                    }
                    WizardUnitDataInfo.GetInstance().WizardCollection[prevID].WizardOperator.Command.OnCanExecuteChanged();
                }
            }
            baseDataContext.NextOperator.Content = I18NEntity.GetString("SATool_NextBtnText");

            #endregion
        }
        private void SetNextPageHandler()
        {
            #region NEXT Behavior
            int prevID = CurrentID;
            CurrentID++;
            if (CurrentID < WizardUnitDataInfo.GetInstance().WizardCollection.Count)
            {
                LeftFrameSource = new Uri(WizardUnitDataInfo.GetInstance().WizardCollection[CurrentID].PageLocation, UriKind.Relative);
                CurrentPage = WizardUnitDataInfo.GetInstance().WizardCollection[CurrentID].PageFeatures;

                WizardUnitDataInfo.GetInstance().WizardCollection[CurrentID].UnitState = WizardUnitState.Configuring;

                WizardUnitDataInfo.GetInstance().WizardCollection[CurrentID].WizardOperator.Command.OnCanExecuteChanged();
                if (CurrentID == WizardUnitDataInfo.GetInstance().WizardCollection.Count - 1)
                {
                    WizardUnitDataInfo.GetInstance().WizardCollection[CurrentID].UnitState = WizardUnitState.Finished;
                }
                if (prevID > -1)
                {
                    WizardUnitDataInfo.GetInstance().WizardCollection[prevID].UnitState = WizardUnitState.Configured;
                    WizardUnitDataInfo.GetInstance().WizardCollection[prevID].IsConfigured = true;
                    WizardUnitDataInfo.GetInstance().WizardCollection[prevID].WizardOperator.Command.OnCanExecuteChanged();
                }
            }
            #endregion
        }

        #region --- Navigation for Auto Switch ---

        public void AutoSwitchPageNext()
        {
            try
            {
                WizardUnitDataInfo.GetInstance().WizardCollection[CurrentID].UnitState = WizardUnitState.Configured;
                WizardUnitDataInfo.GetInstance().WizardCollection[CurrentID].IsConfigured = true;
                WizardUnitDataInfo.GetInstance().WizardCollection[CurrentID].WizardOperator.Command.OnCanExecuteChanged();
                CurrentID++;
                if (CurrentID == 6)
                {
                    LeftFrameSource = new Uri(WizardUnitDataInfo.GetInstance().WizardCollection[CurrentID].PageLocation, UriKind.Relative);
                    CurrentPage = PageFeatures.FinishPage;
                    WizardUnitDataInfo.GetInstance().WizardCollection[CurrentID - 1].UnitState = WizardUnitState.Finished;
                    WizardUnitDataInfo.GetInstance().WizardCollection[CurrentID - 1].IsConfigured = true;
                    WizardUnitDataInfo.GetInstance().WizardCollection[CurrentID - 1].WizardOperator.Command.OnCanExecuteChanged();
                }
                else
                {
                    LeftFrameSource = new Uri(WizardUnitDataInfo.GetInstance().WizardCollection[CurrentID].PageLocation, UriKind.Relative);
                    CurrentPage = WizardUnitDataInfo.GetInstance().WizardCollection[CurrentID].PageFeatures;

                    if (CurrentID == WizardUnitDataInfo.GetInstance().WizardCollection.Count)
                    {
                        WizardUnitDataInfo.GetInstance().WizardCollection[CurrentID].UnitState = WizardUnitState.Finished;
                    }
                    else
                    {
                        WizardUnitDataInfo.GetInstance().WizardCollection[CurrentID].UnitState = WizardUnitState.Configuring;
                    }
                    WizardUnitDataInfo.GetInstance().WizardCollection[CurrentID].WizardOperator.Command.OnCanExecuteChanged();
                }
            }
            catch (Exception ex)
            {
                logger.Warn($"An error occurred while auto switch page with {PageOperation.Next} operation, current page [{CurrentPage}]: {ex}");
            }
        }

        #endregion

        public void NextToPage(PageFeatures page)
        {
            #region NEXT Behavior

            var targetIndex = WizardUnitDataInfo.GetInstance().WizardCollection.ToList().FindIndex(p => p.PageFeatures == page);

            int prevID = CurrentID;
            CurrentID = targetIndex;
            if (CurrentID < WizardUnitDataInfo.GetInstance().WizardCollection.Count)
            {
                LeftFrameSource = new Uri(WizardUnitDataInfo.GetInstance().WizardCollection[CurrentID].PageLocation, UriKind.Relative);
                CurrentPage = WizardUnitDataInfo.GetInstance().WizardCollection[CurrentID].PageFeatures;

                WizardUnitDataInfo.GetInstance().WizardCollection[CurrentID].UnitState = WizardUnitState.Configuring;

                WizardUnitDataInfo.GetInstance().WizardCollection[CurrentID].WizardOperator.Command.OnCanExecuteChanged();
                if (CurrentID == WizardUnitDataInfo.GetInstance().WizardCollection.Count - 1)
                {
                    WizardUnitDataInfo.GetInstance().WizardCollection[CurrentID].UnitState = WizardUnitState.Finished;
                }

                for (int i = prevID; i <= CurrentID - prevID; i++)
                {
                    WizardUnitDataInfo.GetInstance().WizardCollection[i].UnitState = WizardUnitState.Configured;
                    WizardUnitDataInfo.GetInstance().WizardCollection[i].IsConfigured = true;
                    WizardUnitDataInfo.GetInstance().WizardCollection[i].WizardOperator.Command.OnCanExecuteChanged();
                }
            }
            #endregion
        }

        public void BackToPage(PageFeatures page)
        {
            #region NEXT Behavior

            var targetIndex = WizardUnitDataInfo.GetInstance().WizardCollection.ToList().FindIndex(p => p.PageFeatures == page);

            int prevID = CurrentID;
            CurrentID = targetIndex;
            if (CurrentID < 0)
            {
                baseDataContext.ModelCommonInfo.IsCover = false;
                CurrentPage = PageFeatures.WelcomePage;
                baseDataContext.NavigationOperator.CoverFrameSource = new Uri("/View/SelectionPage.xaml", UriKind.Relative);
                baseDataContext.NavigationOperator.HostFrameSource = new Uri("/View/SelectionPage.xaml", UriKind.Relative);
                baseDataContext.NavigationOperator.LeftFrameSource = new Uri("SelectionPage.xaml", UriKind.Relative);
                baseDataContext.NavigationOperator.TopFrameSource = new Uri("WizardPage.xaml", UriKind.Relative);
                return;
            }

            baseDataContext.ModelCommonInfo.IsCover = true;
            while (!WizardUnitDataInfo.GetInstance().WizardCollection[CurrentID].IsEnabled)
            {
                CurrentID--;
                if (CurrentID < 0)
                {
                    break;
                }
            }

            if (CurrentID > -1)
            {
                LeftFrameSource = new Uri(WizardUnitDataInfo.GetInstance().WizardCollection[CurrentID].PageLocation, UriKind.Relative);
                CurrentPage = WizardUnitDataInfo.GetInstance().WizardCollection[CurrentID].PageFeatures;
                WizardUnitDataInfo.GetInstance().WizardCollection[CurrentID].UnitState = WizardUnitState.Configuring;
                WizardUnitDataInfo.GetInstance().WizardCollection[CurrentID].WizardOperator.Command.OnCanExecuteChanged();
                for (var i = prevID; i > currentID; i--)
                {
                    if (i < WizardUnitDataInfo.GetInstance().WizardCollection.Count)
                    {
                        if (WizardUnitDataInfo.GetInstance().WizardCollection[i].IsConfigured)
                        {
                            WizardUnitDataInfo.GetInstance().WizardCollection[i].UnitState = WizardUnitState.Configured;
                        }
                        else
                        {
                            WizardUnitDataInfo.GetInstance().WizardCollection[i].UnitState = WizardUnitState.Waiting;
                        }
                        WizardUnitDataInfo.GetInstance().WizardCollection[i].WizardOperator.Command.OnCanExecuteChanged();
                    }
                }
            }
            baseDataContext.NextOperator.Content = I18NEntity.GetString("SATool_NextBtnText");

            #endregion
        }

    }

    public enum PageOperation
    {
        Next,
        Back
    }
}
