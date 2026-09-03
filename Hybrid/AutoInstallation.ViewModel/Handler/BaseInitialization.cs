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


using System.Windows.Forms;
using AutoInstallation.Contract;
using AutoInstallation.Contract.Interface;
using AutoInstallation.Contract.Interface.Command;
using AutoInstallation.Contract.Interface.Logic;
using AutoInstallation.ViewModel.Command;
using AutoInstallation.ViewModel.CommandButton;

namespace AutoInstallation.ViewModel.Handler
{
    public abstract class BaseInitialization : IInitialization
    {
        protected ApplicationData ContentData;
        protected IInitializationManager SecondaryInit;

        public BaseInitialization(ApplicationData data, IInitializationManager init)
        {
            ContentData = data;
            SecondaryInit = init;
            data.CurrentDirectory = Application.StartupPath;
        }

        public void Init()
        {
            BuildMainWindowViewModel(ContentData.MainWindowViewModel);
            BuildWelcomePageViewModel(ContentData.WelcomeViewModel);
        }

        protected virtual void BuildMainWindowViewModel(IMainWindowViewModel data)
        {
            BuildCancelButton(data);
            BuildNextButton(data);
            BuildBackButton(data);
        }

        //protected virtual void BuildWelcomePageViewModel(IWelcomeViewModel data)
        //{
        //    data.NextButton = ContentData.MainWindowViewModel.NextButton;
        //}
        protected virtual void BuildWelcomePageViewModel(IWelcomeViewModel data)
        {
            data.NextButton = new NextButton();
            data.NextButton.Command =
                new WelcomePageCommand(SecondaryInit, ContentData.MainWindowViewModel.NextButton.Command);
        }

        protected abstract void BuildCancelButton(IMainWindowViewModel data);
        protected abstract void BuildNextButton(IMainWindowViewModel data);

        protected void BuildBackButton(IMainWindowViewModel data)
        {
            IExternalCommand command = new BackCommand(ContentData.NavigationViewModel);
            command.Executed += data.Executed;
            data.BackButton = new BackButton();
            data.BackButton.Command = command;
        }

        //protected virtual void BuildNavigationViewModel(INavigationViewModel data)
        //{
        //    BuildWizardItems(data.Wizards);
        //}
        //protected abstract void BuildWizardItems(ObservableCollection<PageWizardItem> items);
        //protected abstract ILicenseAgreementViewModel BuildLicenseAgreementViewModel();
        //protected abstract PageWizardItem BuildLicenseAgreementWizardItem();
    }
}