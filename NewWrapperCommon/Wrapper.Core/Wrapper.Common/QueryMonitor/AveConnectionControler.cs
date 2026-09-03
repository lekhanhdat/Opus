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
    using System.Data.SqlClient;
    using System.Data;
    using System.Diagnostics;
    using System.Collections;
    #endregion

    internal class AveConnectionControler
    {
        private const string ExecutionTime = "ExecutionTime";
        private const string ConnectionTime = "ConnectionTime";
        private static string processorName;
        private AveQueryWorker connection;
        private int spid;
        private AveMonitorStatistics statistics;
        private long lastStatistics;

        public AveQueryWorker SqlConnection
        {
            get
            {
                return connection;
            }
        }

        public int Id
        {
            get
            {
                return spid;
            }
        }

        public string ProcessorName
        {
            get
            {
                return processorName;
            }
        }

        public AveMonitorStatistics Statistics
        {
            get
            {
                return statistics;
            }
        }

        static AveConnectionControler()
        {
            processorName = Process.GetCurrentProcess().ProcessName;
        }

        public AveConnectionControler(AveQueryWorker aveSqlconn)
        {
            if (aveSqlconn.Connection != null)
            {
                connection = aveSqlconn;
                aveSqlconn.Connection.StatisticsEnabled = true;
                statistics = new AveMonitorStatistics(connection.Connection.RetrieveStatistics() as Hashtable);

                bool needClose = false;
                if (aveSqlconn.Connection.State != ConnectionState.Open)
                {
                    aveSqlconn.Connection.Open();
                    needClose = true;
                }
                spid = AveConnectionMonitorUtil.GetSPID(this);
                if (needClose)
                {
                    aveSqlconn.Connection.Close();
                }
                else
                {
                    if (aveSqlconn.Connection != null)
                    {
                        AveMonitorTyper.WriteToFile(processorName, "SPID: " + spid + "\tConnectionState: Opened" + "\tSqlConnection: " + aveSqlconn.ConnectionString + "\tConnection Time: " + statistics.ConnectionTime);
                    }
                }
                aveSqlconn.Connection.StateChange += ConnectionStateChange;
                aveSqlconn.Command.StatementCompleted += (CommandStatementCompleted);
            }
        }

        private void ConnectionStateChange(object sender, StateChangeEventArgs e)
        {
            try
            {
                SqlConnection sqlConn = sender as SqlConnection;
                if (sqlConn != null)
                {
                    statistics = new AveMonitorStatistics(sqlConn.RetrieveStatistics() as Hashtable);
                    AveMonitorTyper.WriteToFile(processorName, "SPID: " + spid + "\tConnectionState: " + e.CurrentState.ToString() + "\tSqlConnection: " + sqlConn.ConnectionString + "\tConnection Time: " + statistics.ConnectionTime);
                }
            }
            catch (Exception ex)
            {
                AveMonitorTyper.WriteToFile(processorName, "Error Occurred: " + ex.ToString());
            }
        }

        private void CommandStatementCompleted(object sender, StatementCompletedEventArgs e)
        {
            try
            {
                SqlCommand cmd = sender as SqlCommand;
                if (cmd != null)
                {
                    statistics = new AveMonitorStatistics(cmd.Connection.RetrieveStatistics() as Hashtable);
                    long currentExecutionTime = statistics.ExecutionTime - lastStatistics;
                    AveMonitorTyper.WriteToFile(processorName, "SPID: " + spid + "\tElapsed Level: " + CalculateLevel(currentExecutionTime) + "\tExecution Time: " + currentExecutionTime + "\tConnection Time: " + statistics.ConnectionTime + "\tRecordCount: " + e.RecordCount + "\tSqlCommand Text: " + cmd.CommandText);
                    lastStatistics = statistics.ExecutionTime;
                }
            }
            catch (Exception ex)
            {
                AveMonitorTyper.WriteToFile(processorName, "Error Occurred: " + ex.ToString());
            }
        }

        /// <summary>
        /// Calcuate the Level of Elapsed Time,
        /// elapsed Time:       Level:
        ///        0 ~ 100      0
        ///      100 ~ 1000     1
        ///     1000 ~ 10000    2
        ///    10000 ~ 100000   3
        ///   100000 ~ 1000000  4
        ///        ...      ...
        /// </summary>
        /// <param name="durationTime"></param>
        /// <returns></returns>
        private int CalculateElapsedLevel(long durationTime)
        {
            long totalSec = durationTime / 100;
            int level = 0;
            if (totalSec > 0)
            {
                level++;
                totalSec /= 10;
                while (totalSec > 0)
                {
                    level++;
                    totalSec /= 10;
                }
            }
            return level;
        }

        /// <summary>
        /// Calcuate the Level of Elapsed Time;
        ///     ElaspedLevel:     Level:
        ///                0        Low
        ///                1        Normal
        ///              2,3        Middle
        ///               >3        High
        /// </summary>
        /// <param name="durationTime"></param>
        /// <returns></returns>
        private string CalculateLevel(long elapedTime)
        {
            string level;
            switch (CalculateElapsedLevel(elapedTime))
            {
                case 0:
                    level = "Low";
                    break;
                case 1:
                    level = "Normal";
                    break;
                case 2:
                case 3:
                    level = "Middle";
                    break;
                default:
                    level = "High";
                    break;
            }
            return level;
        }
    }
}
