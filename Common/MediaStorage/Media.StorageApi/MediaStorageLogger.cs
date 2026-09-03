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
using AvePoint.GCommon;
using Storage.Util;
using System;


namespace AvePoint.Media.StorageApi
{
    public class MediaStorageLogger :ILogger
    {
        protected IAveLogger Logger { get; set; }
        public MediaStorageLogger(Type t)
        {
            Logger = AveLogger.GetInstance(t);
        }
        public void Debug(string message, params object[] param)
        {
            Logger.Debug(message, param);
        }

        public void Error(string message, params object[] param)
        {
            Logger.Error(message, param);
        }

        public void Info(string message, params object[] param)
        {
            Logger.Info(message, param);
        }

        public void Warn(string message, params object[] param)
        {
            Logger.Warn(message, param);
        }

        public void SetLogLevel(LoggerLevel level)
        {
        
        }
    }
}
