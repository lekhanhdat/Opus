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
using StandaloneTool.Model;
using System.Windows.Input;

namespace StandaloneTool.View.Model.Command
{
    public class BackOperatorCommand : ICommand
    {
        private readonly BaseDataContext context;
        private readonly ImportEncryptionKeyViewModel importEncryptionKeyViewModel = ImportEncryptionKeyViewModel.Instance;

        public BackOperatorCommand(BaseDataContext baseDataContext)
        {
            context = baseDataContext;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            switch (context.NavigationOperator.CurrentPage)
            {
                case PageFeatures.ImportEncryptionKeyPage:
                    importEncryptionKeyViewModel.CleanErrorMessage();
                    return true;
                case PageFeatures.ExportLocationPage:
                    return true;
                case PageFeatures.ProcessPage:
                case PageFeatures.FinishPage:
                    return false;
                default: return true;
            }
        }

        public void Execute(object? parameter)
        {
            context.NavigationOperator.SetCurrentPage(PageOperation.Back);
            context.NextOperator.Command.OnCanExecuteChanged();
            OnCanExecuteChanged();
        }

        public void OnCanExecuteChanged() => CanExecuteChanged.Invoke(this, new EventArgs());
    }
}
