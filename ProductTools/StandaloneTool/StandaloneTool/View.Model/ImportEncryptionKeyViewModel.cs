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
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataExportCore;
using Microsoft.Win32;
using StandaloneTool.Common;
using StandaloneTool.Model.Common;
using StandaloneTool.View.Model.Command;
using System.IO;
using System.Windows;

namespace StandaloneTool.View.Model
{
    public partial class ImportEncryptionKeyViewModel : ObservableObject
    {
        private static readonly Lazy<ImportEncryptionKeyViewModel> instance = new();

        private readonly BaseDataContext context = BaseDataContext.Instance;
        private readonly StringVerification checker = new();

        [ObservableProperty]
        private string encryptionFilePath = string.Empty;
        [ObservableProperty]
        private string encryptionFilePathMsg = string.Empty;
        [ObservableProperty]
        private string encryptionPwd = string.Empty;
        [ObservableProperty]
        private string encryptionPwdMsg = string.Empty;
        [ObservableProperty]
        private string dataType = Module.SharePointOnline.GetEnumDescription();
        [ObservableProperty]
        private bool isCheckingConfig = false;

        public static ImportEncryptionKeyViewModel Instance => instance.Value;



        public void CleanErrorMessage()
        {
            EncryptionFilePathMsg = string.Empty;
            EncryptionPwdMsg = string.Empty;
        }

        partial void OnEncryptionFilePathChanged(string value)
        {
            EncryptionFilePathMsg = checker.VerifyDirectory(EncryptionFilePath) ? string.Empty : I18NEntity.GetString("SATool_EncryptionFilePathInvalidMsg");

            context.NextOperator.Command.OnCanExecuteChanged();
            context.BackOperator.Command.OnCanExecuteChanged();
        }

        [RelayCommand]
        private void IsVisibleChanged()
        {
            context.NextOperator.Command.OnCanExecuteChanged();
            context.BackOperator.Command.OnCanExecuteChanged();
        }

        [RelayCommand]
        private void SelectFile()
        {
            var fileBrowserDialog = new OpenFileDialog { Filter = "Zip Files|*.zip|All Files|*.*" };
            fileBrowserDialog.ShowDialog();
            if (!string.IsNullOrEmpty(fileBrowserDialog.FileName))
            {
                EncryptionFilePath = fileBrowserDialog.FileName;
            }
        }

        [RelayCommand]
        private void PreviewDragOver(DragEventArgs e)
        {
            e.Effects = DragDropEffects.Link;
            e.Handled = true;
        }

        [RelayCommand]
        private void PreviewDrop(DragEventArgs e)
        {
            CleanErrorMessage();
            EncryptionFilePath = string.Empty;
            var path = ((Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            if (Path.GetExtension(path).Equals(".zip", StringComparison.OrdinalIgnoreCase))
            {
                EncryptionFilePath = path;
            }
            else
            {
                EncryptionFilePathMsg = I18NEntity.GetString("SATool_EncryptionFilePathInvalidMsg");
            }
        }
    }
}
