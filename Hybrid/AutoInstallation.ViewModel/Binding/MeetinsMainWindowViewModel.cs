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
using System.Windows;
using System.Windows.Media.Imaging;
using AutoInstallation.Contract;

namespace AutoInstallation.ViewModel.Binding
{
    public abstract class RecordsMainWindowViewModel : BaseMainWindowViewModel
    {
        public RecordsMainWindowViewModel()
        {
            var info = Application.GetResourceStream(new Uri(RecordsVMConstantString.RECORDS_URI_ICONIMAGE,
                UriKind.RelativeOrAbsolute));
            using (var stream = info.Stream)
            {
                var tempImage = new BitmapImage();
                tempImage.BeginInit();
                tempImage.StreamSource = stream;
                tempImage.EndInit();
                Data.IConImage = tempImage;
            }
        }

        protected abstract void StatusController();

        public override void Executed(CommandResult result)
        {
            if (result.ButtonControl == null)
            {
                BackButton.Enabled = true;
                NextButton.Enabled = true;
            }
            else
            {
                BackButton.Enabled = result.ButtonControl.BackButton;
                NextButton.Enabled = result.ButtonControl.NextButton;
                CancelButton.Enabled = result.ButtonControl.CancelButton;
            }

            StatusController();
        }

        public override void Executing()
        {
            BackButton.Enabled = false;
            NextButton.Enabled = false;
        }
    }
}