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


using AutoInstallation.Records.App.Installation.ViewModel.binding;
using System;
using System.Windows.Forms;
using System.Windows.Input;

namespace AutoInstallation.Records.App.Installation.ViewModel
{
    public class SelectConfigurationFileOperatorCommand : ICommand
    {
        private readonly ConfigurationFileData installFolderInfo = ConfigurationFileData.GetInstance();

        public bool CanExecute(object parameter)
        {
            return true;
        }


        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            var openFileBrowserDialog = new OpenFileDialog();

            openFileBrowserDialog.Filter = "All files (*.*)|*.*"; //设置打开文件类型
            //openFileBrowserDialog.Filter = "Text files (*.pfx)|*.pfx|All files (*.*)|*.*"; //设置打开文件类型

            if (openFileBrowserDialog.ShowDialog() == DialogResult.OK)
                installFolderInfo.ConfigFilePath = openFileBrowserDialog.FileName;
            OnCanExecuteChanged();
        }

        public void OnCanExecuteChanged()
        {
            if (CanExecuteChanged != null) CanExecuteChanged(this, new EventArgs());
        }
    }
}