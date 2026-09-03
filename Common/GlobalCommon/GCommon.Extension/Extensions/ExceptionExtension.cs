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



namespace System
{
    #region using directives
    using System.Text;
    #endregion

    public static class ExceptionExtension
    {
        public static String GetExceptionDetail(this Exception exception)
        {
            var detailBuilder = new StringBuilder();
            exception = exception.InnerException ?? exception;
            while (exception != null)
            {
                detailBuilder.AppendFormat(
                   "{0}{1}",
                   exception.ToString(),
                   Environment.NewLine);
                exception = exception.InnerException;
            }
            return detailBuilder.ToString();
        }

        public static String GetExpandedMessage(this Exception exception)
        {
            var messageBuilder = new StringBuilder();
            exception = exception.InnerException ?? exception;
            while (exception != null)
            {
                messageBuilder.AppendFormat(
                    "{0}{1}",
                    exception.Message,
                    Environment.NewLine);
                exception = exception.InnerException;
            }
            return messageBuilder.ToString();
        }

        public static String GetRawMessage(this Exception exception)
        {
            var rawException = exception.InnerException ?? exception;
            return rawException.Message;
        }
    }
}