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

namespace Microsoft365.Common.Logger
{
    public interface IMicrosoft365Logger
    {
        void Debug(string message, params object[] param);
        void Info(string message, params object[] param);
        void Error(string message, params object[] param);
        void Warn(string message, params object[] param);
        void Trace(string message, params object[] param);
    }
    public interface ILoggerFactory
    {
        IMicrosoft365Logger GetLogger(Type t);
    }

    //public class LoggingOption
    //{
    //    public LoggingEvent Debug { get; set; }
    //    public LoggingEvent Info { get; set; }
    //    public LoggingEvent Error { get; set; }
    //    public LoggingEvent Warn { get; set; }
    //    public LoggingEvent Trace { get; set; }
    //}
}