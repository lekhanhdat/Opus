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
using AvePoint.GCommon.Contract.AccountManager.Object;

namespace DataExportCore.Utils
{
    public class ManagedException : Exception
    {
        public ErrorType ErrorType { get; set; }

        public ManagedException() : base()
        {
        }

        public ManagedException(string message)
            : base(message)
        {
        }

        public ManagedException(ErrorType errorType, params string[]? values) : base(GetDefaultMessage(errorType, values))
        {
            ErrorType = errorType;
        }

        public ManagedException(ErrorType errorType, string errorMessage)
            : base(errorMessage)
        {
            ErrorType = errorType;
        }

        private static string GetDefaultMessage(ErrorType code, string[]? values)
        {
            var baseMessage = code switch
            {
                ErrorType.SubJobNotFound => I18NEntity.GetString("SATool_SubJobNotFoundError"),
                ErrorType.CannotOpenDevice => I18NEntity.GetString("SATool_CannotOpenDeviceError"),
                ErrorType.DeviceNotFound => I18NEntity.GetString("SATool_DeviceNotFoundError"),
                _ => I18NEntity.GetString("SATool_ExportItemUnexpectedError")
            };

            return FormatMessage(baseMessage, values);
        }

        private static string FormatMessage(string message, string[]? values)
        {
            if (values == null || values.Length == 0)
            {
                return message;
            }

            try
            {
                return string.Format(message, values);
            }
            catch (FormatException)
            {
                return message;
            }
        }
    }

    public enum ErrorType
    {
        SubJobNotFound,
        CannotOpenDevice,
        DeviceNotFound
    }
}
