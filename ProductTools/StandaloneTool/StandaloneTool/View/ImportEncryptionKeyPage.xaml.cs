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
using DataExportCore;
using StandaloneTool.Model.Common;
using StandaloneTool.View.Model;
using System.Windows.Controls;

namespace StandaloneTool.View
{
    public partial class ImportEncryptionKeyPage : Page
    {
        private ImportEncryptionKeyViewModel importEncryptionKeyViewModel = ImportEncryptionKeyViewModel.Instance;

        private List<string> dataTypes = new List<string>
        {
            I18NEntity.GetString("SATool_DataType_SharePointOnline"),
            I18NEntity.GetString("SATool_DataType_OneDrive"),
            I18NEntity.GetString("SATool_DataType_Teams")

        };

        public ImportEncryptionKeyPage()
        {
            InitializeComponent();
            DataContext = importEncryptionKeyViewModel;
            PreSetupPage();
        }

        private void PreSetupPage()
        {
            dataTypes.ForEach(d => comboBoxDataType.Items.Add(d));
            comboBoxDataType.SelectedValue = I18NEntity.GetString("SATool_DataType_SharePointOnline");
            importEncryptionKeyViewModel.CleanErrorMessage();
        }

        private void ComboBoxDataType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedText = comboBoxDataType.SelectedValue?.ToString() ?? importEncryptionKeyViewModel.DataType;
            importEncryptionKeyViewModel.DataType = selectedText;

            if (selectedText.Equals(I18NEntity.GetString("SATool_DataType_OneDrive"), StringComparison.OrdinalIgnoreCase))
            {
                GlobalInfo.Module = Module.OneDrive;
            }
            else if (selectedText.Equals(I18NEntity.GetString("SATool_DataType_SharePointOnline"), StringComparison.OrdinalIgnoreCase))
            {
                GlobalInfo.Module = Module.SharePointOnline;
            }
            else if (selectedText.Equals(I18NEntity.GetString("SATool_DataType_Teams"), StringComparison.OrdinalIgnoreCase))
            {
                GlobalInfo.Module = Module.Teams;
            }
        }

    }
}
