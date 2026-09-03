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




namespace AvePoint.Wrapper.Common
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Diagnostics;
    using System.IO;
    using System.Data.SqlClient;
    using System.Threading;
    using System.Collections;
    using System.Data;
    using System.Reflection;
    using AvePoint.Common;
    using AvePoint.GCommon;
    using AvePoint.Wrapper.Resource;
    #endregion

    public class AveQueryMonitor
    {
        private static AveLogger log = AveLogger.GetInstance(typeof(AveQueryMonitor));
        private static int defaultTimeOut;
        private static int checkInterval;
        private static List<AveConnectionControler> monitoredConnectionSet = new List<AveConnectionControler>();
        private static Thread timeoutChecker;
        private readonly static object locker = new object();

        static AveQueryMonitor()
        {
            defaultTimeOut = AveSqlConnection.DEFAULT_TIMEOUT;
            checkInterval = WrapperConfiguration.CheckInterval;

            timeoutChecker = new Thread(CheckConnectionTimeOut);
            timeoutChecker.Name = "Connection Timeout Checker";
            timeoutChecker.IsBackground = true;
            timeoutChecker.Start();
        }

        /// <summary>
        /// register connection for monitoring
        /// </summary>
        /// <param name="conn"></param>
        public static void RegisterConnection(AveSqlConnection aveSqlconn)
        {
            try
            {
                lock (locker)
                {
                    monitoredConnectionSet.Add(new AveConnectionControler(aveSqlconn));
                }
            }
            catch (Exception ex)
            {
                log.Debug("An error occurred when Register Query Monitor; Message: {0}", ex.ToString());
            }
        }

        private static void CheckConnectionTimeOut()
        {
            try
            {
                while (true)
                {
                    lock (locker)
                    {
                        // log.Debug(check begin)                
                        List<AveConnectionControler> timeoutConnList = new List<AveConnectionControler>();
                        List<AveConnectionControler> closedConnList = new List<AveConnectionControler>();
                        foreach (AveConnectionControler conn in monitoredConnectionSet)
                        {
                            if (conn.SqlConnection.Connection == null || conn.SqlConnection.Connection.State == ConnectionState.Closed)
                            {
                                closedConnList.Add(conn);
                            }
                            else if (conn.Statistics.ConnectionTime > defaultTimeOut * 1000)
                            {
                                timeoutConnList.Add(conn);
                            }
                        }
                        if (closedConnList.Count > 0)
                        {
                            foreach (AveConnectionControler closedConn in closedConnList)
                            {
                                monitoredConnectionSet.Remove(closedConn);
                            }
                        }
                        if (timeoutConnList.Count > 0)
                        {
                            DumpTimeoutConnection(timeoutConnList);
                        }
                    }
                    //check it every 10 sec.
                    Thread.Sleep(checkInterval * 1000);
                }
                // log.Debug(check complete);
            }
            catch (Exception ex)
            {
                //make sure this thread won't corrupt whole process
                log.Log(AveLogLevel.WARN, WrapperCommonResource.AWCConnectionTimeOutError, ex.ToString());
            }
        }

        private static void DumpTimeoutConnection(List<AveConnectionControler> timeoutConns)
        {
            foreach (AveConnectionControler conn in timeoutConns)
            {
                monitoredConnectionSet.Remove(conn);
                //write dumpinfo to log or files
                AveMonitorTyper.WriteToFile(conn.ProcessorName, "SPID: " + conn.Id + "\tConnection TimeOut" + "\tDumpSysProcess: " + AveConnectionMonitorUtil.DumpSysProcess(conn));
                AveMonitorTyper.WriteToFile(conn.ProcessorName, "SPID: " + conn.Id + "\tConnection TimeOut" + "\tDumpLocksInfo: " + AveConnectionMonitorUtil.DumpLocksInfo(conn));
            }
        }
    }
}
