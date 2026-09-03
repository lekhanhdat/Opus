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
using System.Globalization;
using System.Windows.Data;
using AutoInstallation.Contract.Message;

namespace AutoInstallation.Common.GUI
{
    public class MessageWindowIconTypeIconImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = "Images/msgwin_done_32x32.png";
            switch ((MessageIconType) value)
            {
                case MessageIconType.Done:
                    result = "Images/msgwin_done_32x32.png";
                    break;
                case MessageIconType.Error:
                    result = "Images/msgwin_error_32x32.png";
                    break;
                case MessageIconType.Warning:
                    result = "Images/msgwin_warning_32x32.png";
                    break;
                case MessageIconType.Exit:
                    result = "Images/cfmwin_exit_32x32.png";
                    break;
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }


    //public enum MessageIconType
    //{
    //    Done,
    //    Error,
    //    Warning,
    //    Exit
    //}
}