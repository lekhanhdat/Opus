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


namespace AutoInstallationCommon.Utility
{
    #region using directives

    #endregion

    public interface IAveLogger
    {
        AveLogLevel CurrentLogLevel { get; }
        bool IsErrorEnabled { get; }
        bool IsWarnEnabled { get; }
        bool IsInfoEnabled { get; }
        bool IsDebugEnabled { get; }

        void Error(string formatStr, params object[] args);
        void Error(int eventId, string formatStr, params object[] args);
        void Error(ushort taskCategory, int eventId, string formatStr, params object[] args);
        void Error(string eventSource, ushort taskCategory, int eventId, string formatStr, params object[] args);

        void Warn(string formatStr, params object[] args);
        void Warn(int eventId, string formatStr, params object[] args);
        void Warn(ushort taskCategory, int eventId, string formatStr, params object[] args);
        void Warn(string eventSource, ushort taskCategory, int eventId, string formatStr, params object[] args);

        void Info(string formatStr, params object[] args);
        void Info(int eventId, string formatStr, params object[] args);
        void Info(ushort taskCategory, int eventId, string formatStr, params object[] args);
        void Info(string eventSource, ushort taskCategory, int eventId, string formatStr, params object[] args);

        void Debug(string formatStr, params object[] args);
        void Debug(int eventId, string formatStr, params object[] args);
        void Debug(ushort taskCategory, int eventId, string formatStr, params object[] args);
        void Debug(string eventSource, ushort taskCategory, int eventId, string formatStr, params object[] args);

        void Log(AveLogLevel aveLogLevel, string formatStr, params object[] args);
        void Log(AveLogLevel aveLogLevel, int eventId, string formatStr, params object[] args);
        void Log(AveLogLevel aveLogLevel, ushort taskCategory, int eventId, string formatStr, params object[] args);

        void Log(AveLogLevel aveLogLevel, string eventSource, ushort taskCategory, int eventId, string formatStr,
            params object[] args);
    }
}