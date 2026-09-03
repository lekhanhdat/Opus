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


using System.Windows;
using System.Windows.Media;
using COMMONRESX = AutoInstallation.Records.App.Resources.Resource;

namespace AutoInstallation.Contract.Message
{
    public class BaseMessageConfig
    {
        public Window OwnerWindow { get; set; }

        public string FormText { get; set; }

        public string ContentTitle { get; set; }

        public object ContentSummary { get; set; }

        public MessageType MessageType { get; set; }

        public MessageIconType MessageIconType { get; set; }

        public ImageSource Icon { get; set; }

        public string OkContent { get; set; } /*= COMMONRESX.COMMON_BTN_OK;*/

        public string DetailContent { get; set; } = string.Empty;

        public string UpdateContent { get; set; } = string.Empty;

        public string RetryContent { get; set; } = string.Empty;

        public string CancelContent { get; set; } /*= COMMONRESX.COMMON_BTN_CANCEL;*/

        public string IgnoreContent { get; set; } = string.Empty;
    }


    public enum MessageType
    {
        YesNoCancel,
        OK,
        DetailOK,
        OKCancel,
        RetryIgnoreCancel,
        DetailUpdateOk,
        Test,
        RetryCancel,
        OKCancel1
    }

    public enum MessageResult
    {
        Yes,
        No,
        OK,
        Detail,
        Update,
        Retry,
        Ignore,
        Cancel,
        Test,
        None
    }

    public enum MessageIconType
    {
        Done,
        Error,
        Warning,
        Exit
    }
}