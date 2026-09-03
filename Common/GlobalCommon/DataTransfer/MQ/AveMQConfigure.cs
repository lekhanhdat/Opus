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



using AvePoint.GCommon.Transfer.Common;

namespace AvePoint.GCommon.Transfer.MQ
{
    /// <summary>
    /// MQ的一些配置信息。
    /// </summary>
    public class AveMQConfigure
    {
        public static string TempFolder = string.Empty;
        public static int MaxMessageTimeout = 48 * (60 * 60 * 1000);//48h
        public static AveChannelMode MQChannelMode = AveChannelMode.WCF;
        public static bool IsOneWayConnection = false;
        public static int MaxRetryTimes = 3;
        public static int MaxReconnectionTimeOut = 1800;//断网重连 timeout时间 单位为秒
        public static int ReconnectionTime = 2;//断网重连间隔 单位为秒
        public static int NoReconnectTimeOut = 3;//上次重连结束之后在该段时间内出现的重连请求不处理
        /// <summary>
        /// MQ相关的Performance Counter信息
        /// </summary>
        public static bool EnablePerformanceCounter = true;

        static AveMQConfigure()
        {
            XmlConfiguration.InitiateConfiguration();
            //try
            //{
            //    TempFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "temp");

            //    if (!Directory.Exists(TempFolder))
            //    {
            //        Directory.CreateDirectory(TempFolder);
            //    }
            //}
            //catch
            //{
            //    TempFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            //}
        }
    }
}