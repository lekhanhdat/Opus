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
using System.Windows.Input;
using AutoInstallation.Contract;
using AutoInstallation.Contract.Interface.Logic;
using AutoInstallationCommon.Utility;

namespace AutoInstallation.ViewModel.Command
{
    public class WelcomePageCommand : ICommand
    {
        private static readonly AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ICommand command;

        private readonly IInitializationManager manager;

        //public WelcomePageCommand(IInitializationManager _manager, InstallationModel _model,ICommand _command)
        //{
        //    manager = _manager;
        //    model = _model;
        //    command = _command;
        //}
        public WelcomePageCommand(IInitializationManager _manager, ICommand _command)
        {
            manager = _manager;
            command = _command;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            try
            {
                var model = InstallationModel.Install;
                if (parameter != null) model = (InstallationModel) parameter;
                switch (model)
                {
                    case InstallationModel.Install:
                        manager.InstallInit.Init();
                        break;
                    case InstallationModel.Upgrade:
                        manager.UpgradeInit.Init();
                        break;
                    case InstallationModel.Uninstall:
                        manager.UninstallInit.Init();
                        break;
                    case InstallationModel.GeneratePackage:
                        manager.GeneratePackageInit.Init();
                        break;
                    default:
                        break;
                }

                command.Execute(null);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
        }
    }
}