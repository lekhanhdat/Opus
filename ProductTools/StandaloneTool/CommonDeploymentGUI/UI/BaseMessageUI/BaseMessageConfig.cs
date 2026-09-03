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

namespace AvePoint.Deployment.CommonGUI
{
    #region ---namespace---

    using System.Windows;

    #endregion

    public class BaseMessageConfig
    {
        private string formText;

        public string FormText
        {
            get { return formText; }
            set { formText = value; }
        }
        private string contentTitle;

        public string ContentTitle
        {
            get { return contentTitle; }
            set { contentTitle = value; }
        }
        private object contentSummary;

        public object ContentSummary
        {
            get { return contentSummary; }
            set { contentSummary = value; }
        }
        private MessageType messageType;

        public MessageType MessageType
        {
            get { return messageType; }
            set { messageType = value; }
        }
        private MessageIconType messageIconType;

        public MessageIconType MessageIconType
        {
            get { return messageIconType; }
            set { messageIconType = value; }
        }
    }


    public enum MessageType
    {
        YesNo,
        YesNoCancel,
        OK,
        DetailOK,
        OKCancel,
        OKCancel1,
        RetryIgnoreCancel,
        Test
    }

    public enum MessageResult
    {
        Yes,
        No,
        OK,
        Detail,
        Retry,
        Ignore,
        Cancel,
        Test,
        None
    }
}