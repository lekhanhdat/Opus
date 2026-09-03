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
using System.Windows.Controls;
using StandaloneTool.Model.Common;
using AvePoint.RA.Common.Util;
using StandaloneTool.View.Model;
using System.Windows;
using System.Windows.Threading;
using AvePoint.RA.CommonUtil;
using StandaloneTool.View.Model.Command;
using DataExportCore;

namespace StandaloneTool.View
{
    public partial class ExportLocationPage : Page
    {
        private RALogger logger = RALogger.GetInstance(typeof(ExportLocationPage));
        private readonly ExportLocationViewModel ExportLocationViewModel = ExportLocationViewModel.Instance;
        private readonly BaseDataContext context = BaseDataContext.Instance;
          
        public ExportLocationPage()
        {
            InitializeComponent();
            DataContext = ExportLocationViewModel;
            SetupPage();
        }

        private void SetupPage()
        {
            GlobalInfo.SftpPrivateKeyFileContent = string.Empty;
            ExportLocationViewModel.CleanMessage();
            ExportLocationViewModel.LocationType = LocationType.LocalLocation.ToDescription();
            Enum.GetValues(typeof(LocationType)).Cast<LocationType>().ForEach(l => comboBoxLocationType.Items.Add(l.ToDescription()));
            comboBoxLocationType.SelectedValue = LocationType.LocalLocation.ToDescription();
            context.NextOperator.Content = I18NEntity.GetString("SATool_ExportBtnText");
        }

        private void ComboBoxLocationType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ExportLocationViewModel.IsCheckingConfig = true;
                var checkThread = new Thread(ComboBoxLocationTypeOnChanged);
                checkThread.SetApartmentState(ApartmentState.STA);
                checkThread.IsBackground = true;
                checkThread.Start();
            }
            catch (Exception ex)
            {
                logger.Warn("Action change for location type failed: {0}.", ex);
            }
        }

        private void ComboBoxLocationTypeOnChanged(object sender)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                ExportLocationViewModel.CleanMessage();
                ExportLocationViewModel.RevertSelection();
                var selectedText = comboBoxLocationType.SelectedValue?.ToString() ?? ExportLocationViewModel.LocationType;
                if (string.Equals(selectedText, LocationType.MSAzureBlob.ToDescription(), StringComparison.OrdinalIgnoreCase))
                {
                    ExportLocationViewModel.IsSelectedAzure = true;
                    GlobalInfo.ExportOption = LocationType.MSAzureBlob;
                }
                else if (string.Equals(selectedText, LocationType.SFTP.ToDescription(), StringComparison.OrdinalIgnoreCase))
                {
                    ExportLocationViewModel.IsSelectedSftp = true;
                    GlobalInfo.ExportOption = LocationType.SFTP;
                }
                else
                {
                    ExportLocationViewModel.IsSelectedLocal = true;
                    GlobalInfo.ExportOption = LocationType.LocalLocation;
                    GlobalInfo.TargetStorageType = AvePoint.GCommon.Contract.Storage.Entity.StorageDeviceType.None;
                }
            }), DispatcherPriority.Normal);

            Thread.Sleep(300);
            ExportLocationViewModel.IsCheckingConfig = false;
        }

        private void StackPanel_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            context.NextOperator.Command.OnCanExecuteChanged();
            context.BackOperator.Command.OnCanExecuteChanged();
        }

        private void TextBoxPassword_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;

            if (textBox == null) return;

            if (!textBox.Text.Contains("*")) textBox.Tag = textBox.Text;

            if (textBox.Tag is string originalText)
            {
                if (textBox.Text.Length < originalText.Length)
                {
                    originalText = originalText.Substring(0, textBox.Text.Length);
                }
                else
                {
                    var newChars = textBox.Text.Substring(originalText.Length);
                    originalText += newChars;
                }

                textBox.Tag = originalText;
                ExportLocationViewModel.sftpPasswordCache = originalText;
            }
            else
            {
                textBox.Tag = textBox.Text;
            }
            textBox.Text = new string('*', textBox.Text.Length);
            textBox.CaretIndex = textBox.Text.Length;
        }
    }
}
