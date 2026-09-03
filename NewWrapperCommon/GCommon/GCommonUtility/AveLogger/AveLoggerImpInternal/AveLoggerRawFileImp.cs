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



namespace AvePoint.GCommon
{
    #region using directives
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Xml;
    using log4net;
    using log4net.Appender;
    using log4net.Config;
    using log4net.Core;
    using log4net.Repository;
    using log4net.Repository.Hierarchy;
    using System.Collections.Generic;
    using System.Threading;
    using System.Reflection;
    using System.Configuration;

    #endregion

    /// <summary>
    /// 日志类。该类可以将log写入文件.
    /// </summary>
    internal class AveLoggerFileImp : AveLoggerAbstractImp
    {
        public override void WriteEntry(string msg, AveLogLevel level, int eventId, ushort taskCategory, string eventSource, Exception e)
        {
           
        }
    }

}