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
using StandaloneTool.View.Model.Binding;
using StandaloneTool.View.Model.Handler;
using System.Windows.Input;

namespace StandaloneTool.View.Model.Command
{
    public class NextOperatorCommand : ICommand
    {
        private readonly BaseDataContext context;
        private readonly ExchangeDataInfo exchangeInfo = ExchangeDataInfo.GetInstance();
        private readonly ExportLocationViewModel exportLocationViewModel = ExportLocationViewModel.Instance;
        private readonly ImportEncryptionKeyViewModel importEncryptionKeyViewModel = ImportEncryptionKeyViewModel.Instance;
        private readonly StorageInformationViewModel storageInformationViewModel = StorageInformationViewModel.Instance;
        private readonly DatabaseHelper dbHelper = DatabaseHelper.Instance;
        public event EventHandler? CanExecuteChanged;

        public NextOperatorCommand(BaseDataContext baseDataContext)
        {
            context = baseDataContext;
        }

        public bool CanExecute(object parameter = null)
        {
            switch (context.NavigationOperator.CurrentPage)
            {
                case PageFeatures.StorageInformationPage:
                    return !storageInformationViewModel.IsCheckingConfig;
                case PageFeatures.ImportEncryptionKeyPage:
                    return !importEncryptionKeyViewModel.IsCheckingConfig;
                case PageFeatures.ExportLocationPage:
                    return string.IsNullOrEmpty(exportLocationViewModel.ExportLocationErrorMsg);
                case PageFeatures.RecoveryPage:
                    return exchangeInfo.SelectionList.Any();
                case PageFeatures.ProcessPage:
                    context.NextOperator.IsEnabled = false;
                    return false;
                case PageFeatures.FinishPage:
                    return true;
            }
            return true;
        }

        public void Execute(object parameter = null)
        {
            context.ModelCommonInfo.IsCover = true;
            switch (context.NavigationOperator.CurrentPage)
            {
                case PageFeatures.ImportEncryptionKeyPage:
                    new VerifyFileInfoHandler().Execute();
                    return;
                case PageFeatures.RecoveryPage:
                    if (dbHelper.CheckUsingAveStorage(exchangeInfo.SelectionList.Select(i => i.SiteUrl))) break;
                    context.NavigationOperator.NextToPage(PageFeatures.ExportLocationPage);
                    return;
                case PageFeatures.StorageInformationPage:
                    new VerifyStorageInfoHandler().Execute();
                    return;
                case PageFeatures.ExportLocationPage:
                    new VerifyExportLocationHandler().Execute();
                    return;
                case PageFeatures.FinishPage:
                    System.Windows.Application.Current.Shutdown();
                    break;
            }

            context.NavigationOperator.HostFrameSource = new Uri("View/MainContentTemplate.xaml", UriKind.Relative);
            context.NavigationOperator.SetCurrentPage(PageOperation.Next);
            context.NextOperator.Command.OnCanExecuteChanged();
            context.BackOperator.Command.OnCanExecuteChanged();
            OnCanExecuteChanged();
        }


        public void OnCanExecuteChanged() => CanExecuteChanged?.Invoke(this, new EventArgs());
    }
}
