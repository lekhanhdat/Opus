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
using System.Reflection;
using System.Windows;
using AutoInstallation.Contract;
using AutoInstallation.Contract.EventHandel;
using AutoInstallation.Contract.Interface.Command;
using AutoInstallation.Contract.Interface.Navigation;
using AutoInstallation.Contract.Navigation;
using AutoInstallationCommon.Utility;
using LOGRESX = AutoInstallation.Records.App.Resources.LogResource;

namespace AutoInstallation.ViewModel.Command
{
    public class BaseNextCommand : IExternalCommand
    {
        private static readonly AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly INavigationViewModel nav;

        public BaseNextCommand(INavigationViewModel _nav)
        {
            nav = _nav;
        }

        public event AutoInstallEventHandler.ExecutingEventHandler Executing;
        public event AutoInstallEventHandler.ExecutedEventHandler Executed;
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            try
            {
                ILogicWizardItem item = nav.CurrentItem;
                if (item != null)
                {
                    if (item.Type == WizardLogicType.FinishPage)
                    {
                        Application.Current.Shutdown();
                    }
                    else if (nav.CurrentIndex == -1)
                    {
                        var ret = new CommandResult(true, null);
                        AfterExecute(ret);
                    }
                    else
                    {
                        var command = item.VerifySelf as IExternalCommand;
                        if (command != null)
                        {
                            command.Executing -= BeforeExecute;
                            command.Executing += BeforeExecute;
                            command.Executed -= AfterExecute;
                            command.Executed += AfterExecute;
                            command.Execute(null);
                        }
                        else
                        {
                            if (item.VerifySelf != null) item.VerifySelf.Execute(null);
                            //nav.Next();
                            //if (Executed != null)
                            //{
                            //    CommandResult ret = new CommandResult(true, null);
                            //    Executed(ret);
                            //}
                            var ret = new CommandResult(true, null);
                            AfterExecute(ret);
                        }
                    }
                }
                else
                {
                    //nav.Next();
                    //if (Executed != null)
                    //{
                    //    CommandResult ret = new CommandResult(true, null);
                    //    Executed(ret);
                    //}
                    var ret = new CommandResult(true, null);
                    AfterExecute(ret);
                }
            }
            catch (Exception ex)
            {
                logger.Error(LOGRESX.COMMONLOG_NEXTCOMMANDERROR, ex.ToString());
            }
        }

        private void AfterExecute(CommandResult result)
        {
            if (result.IsSuccessful)
            {
                nav.Next();
                if (Executed != null) Application.Current.Dispatcher.BeginInvoke(Executed, result);
                if (nav.CurrentItem.Type == WizardLogicType.ProgressPage) Execute(null);
            }
            else
            {
                if (Executed != null) Application.Current.Dispatcher.BeginInvoke(Executed, result);
            }

            //Application.Current.Dispatcher.BeginInvoke(new ChangeCanExecuteInternal(OnCanExecuteChanged));
            //Application.Current.Dispatcher.BeginInvoke(new ChangeCanExecuteInternal(mainWindowData.BackButton.Command.OnCanExecuteChanged));
            //Application.Current.Dispatcher.BeginInvoke(new ChangeCanExecuteInternal(mainWindowData.CancelButton.Command.OnCanExecuteChanged));
        }

        private void BeforeExecute()
        {
            if (Executing != null) Application.Current.Dispatcher.BeginInvoke(Executing);
        }
    }
}