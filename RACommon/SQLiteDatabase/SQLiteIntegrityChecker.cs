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


namespace RACommon.SQLiteDatabase;

using AvePoint.RA.CommonUtil;

using System;
using System.Data.SQLite;
using System.Diagnostics;

public class SQLiteIntegrityChecker
{
    private static RALogger logger = RALogger.GetInstance(typeof(SQLiteIntegrityChecker));
    public static int IntegrityCheck(string indexDbpath)
    {
        logger.Info("Start to check index db integrity. FilePath: {0}", indexDbpath);
        var monitor = Stopwatch.StartNew();
        string scanResult;
        try
        {
            using (var connection = new SQLiteConnection($@"Data Source= {indexDbpath}"))
            {
                using (var command = new SQLiteCommand())
                {
                    command.Connection = connection;
                    command.CommandText = @"PRAGMA quick_check";
                    command.CommandType = System.Data.CommandType.Text;
                    command.CommandTimeout = 30;
                    if (connection.State != System.Data.ConnectionState.Open)
                    {
                        connection.Open();
                    }
                    scanResult = Convert.ToString(command.ExecuteScalar()) ?? string.Empty;
                }
            }
            var result = scanResult.Equals("OK", StringComparison.OrdinalIgnoreCase);
            logger.Info($"Finish to check index db integrity before commit db, the check result is {result}, TimeCost:{monitor.Elapsed}, indexpath:{indexDbpath}");
            return result ? 0 : 1;
        }
        catch (Exception ex)
        {
            logger.Warn("An error occurred while to check index db integrity. Reason: {0}.", ex);
            return 1;
        }
    }
}