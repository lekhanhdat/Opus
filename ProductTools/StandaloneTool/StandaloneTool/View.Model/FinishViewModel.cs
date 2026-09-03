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
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataExportCore;
using DataExportCore.Resources;
using StandaloneTool.Model.Common;
using StandaloneTool.View.Model.Command;
using System.Diagnostics;
using System.Windows.Media.Imaging;

namespace StandaloneTool.View.Model
{
    public partial class FinishViewModel : ObservableObject
    {
        private readonly RALogger logger = RALogger.GetInstance(typeof(FinishViewModel));
        private readonly BaseDataContext context = BaseDataContext.Instance;

        [ObservableProperty]
        private bool onlySupportExchangeAndNotSpecialCustomer = true;
        [ObservableProperty]
        private string exportSize;
        [ObservableProperty]
        private BitmapFrame finishImageSource;
        [ObservableProperty]
        private string finishText;

        public FinishViewModel()
        {
            context.NextOperator.Command.OnCanExecuteChanged();
            context.BackOperator.Command.OnCanExecuteChanged();
            //context.CancelOperator.Command.OnCanExecuteChanged();

            if (GlobalInfo.FinalJobStatus == JobStatus.Finished)
            {
                FinishImageSource = BitmapFrame.Create(new Uri("pack://application:,,,/Images/successful.png", UriKind.RelativeOrAbsolute));
                FinishText = CommonResourceManager.SATool_Completed;
            }
            else if (GlobalInfo.FinalJobStatus == JobStatus.Failed)
            {
                FinishImageSource = BitmapFrame.Create(new Uri("pack://application:,,,/Images/failed.png", UriKind.RelativeOrAbsolute));
                FinishText = CommonResourceManager.SATool_Failed;
            }
            else
            {
                FinishImageSource = BitmapFrame.Create(new Uri("pack://application:,,,/Images/finish_with_exception.png", UriKind.RelativeOrAbsolute));
                FinishText = CommonResourceManager.SATool_FinishedWithException;
            }

            CalculateTotalSize();
        }

        public string ExportFolder => GlobalInfo.ExportOption == LocationType.LocalLocation ? GlobalCache.ExportLocation : GlobalInfo.ExportLocation;

        public string ExportOption => string.Format(CommonResourceManager.SATool_ExportPathText, GetExportOptionText());

        private void CalculateTotalSize()
        {
            var (size, unit) = SizeUtil.AutoFitSizeUnit(GlobalInfo.TotalExportedSize);
            ExportSize = $"{Math.Round(size, 2)} {(unit == SizeUtil.SizeUnit.Byte && size > 1 ? "Bytes" : unit.ToString())}";
        }

        private string GetExportOptionText()
        {
            switch (GlobalInfo.ExportOption)
            {
                case LocationType.LocalLocation:
                    return LocationType.LocalLocation.GetEnumDescription();
                case LocationType.MSAzureBlob:
                    return LocationType.MSAzureBlob.GetEnumDescription();
                case LocationType.SFTP:
                    return LocationType.SFTP.GetEnumDescription(); ;
                default: 
                    return string.Empty;
            }
        }

        [RelayCommand]
        private void OpenLogFile()
        {
            try
            {
                using var process = new Process();
                process.StartInfo = new ProcessStartInfo(GlobalCache.ExportLocation) { UseShellExecute = true };
                process.Start();
            }
            catch (Exception ex)
            {
                logger.Error("Open log file failed. Error message: {0}.", ex);
            }
        }
    }
}
