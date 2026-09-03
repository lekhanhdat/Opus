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
using System.Windows.Media;
using AutoInstallation.Contract.Message;
using GUIRESX = AutoInstallation.Records.App.Resources.Resource;

namespace AutoInstallation.ViewModel.Handler
{
    public class PopupMessageBox
    {
        private static readonly PopupMessageBox instance = new PopupMessageBox();
        public ImageSource mIcon;

        public static PopupMessageBox GetInstance()
        {
            return instance;
        }

        public static PopupMessageBox GetInstance(ImageSource icon)
        {
            instance.mIcon = icon;
            return instance;
        }

        public void ShowWarningMessageBox(string summary)
        {
            var messageResult = BaseMessage.Show(
                new BaseMessageConfig
                {
                    FormText = "Records",
                    ContentTitle = "Records",
                    ContentSummary = summary,
                    MessageType = MessageType.OK,
                    MessageIconType = MessageIconType.Warning,
                    Icon = mIcon,
                    //OkContent = GUIRESX.COMMON_BTN_OK
                });
            Environment.Exit(0);
        }

        public void ShowWarningMessageBoxNotExit(string summary)
        {
            var messageResult = BaseMessage.Show(
                new BaseMessageConfig
                {
                    FormText = "Records",
                    ContentTitle = "Records",
                    ContentSummary = summary,
                    MessageType = MessageType.OK,
                    MessageIconType = MessageIconType.Warning,
                    Icon = mIcon,
                    //OkContent = GUIRESX.COMMON_BTN_OK
                });
        }

        public void ShowErrorMessageBox(string summary)
        {
            var messageResult = BaseMessage.Show(
                new BaseMessageConfig
                {
                    FormText = "Records",
                    ContentTitle = "Records",
                    ContentSummary = summary,
                    MessageType = MessageType.OK,
                    MessageIconType = MessageIconType.Error,
                    Icon = mIcon,
                    //OkContent = GUIRESX.COMMON_BTN_OK
                });
            Environment.Exit(0);
        }

        public MessageResult ShowMessageBox(string summary, string okButton, string cancelContent)
        {
            var title = string.Empty;
            var result = BaseMessage.Show(new BaseMessageConfig
            {
                FormText = "Records",
                ContentTitle = "Records",
                ContentSummary = summary,
                MessageType = MessageType.OKCancel1,
                MessageIconType = MessageIconType.Exit,
                Icon = mIcon,
                OkContent = okButton,
                CancelContent = cancelContent
            });
            return result;
        }
    }
}