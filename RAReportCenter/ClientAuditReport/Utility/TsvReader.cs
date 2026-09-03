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
using AvePoint.RA.CommonUtil;
using System;
using System.IO;
using System.Text;


namespace RAReportCenter.ClientAuditReport.Scanner
{
    public class TsvReader : IDisposable
    {
        private StreamReader reader;
        public const char Separator = '\t';
        private string[] fieldValues;
        public int Count;

        public TsvReader(string file)
        {
            ThrowUtil.ThrowIfNull(file, "file");
            reader = new StreamReader(file, Encoding.UTF8);
        }

        public bool Read()
        {
            var currentLine = reader.ReadLine();
            if (currentLine != null)
            {
                fieldValues = currentLine.Split(Separator);
                Count = fieldValues.Length;
                return true;
            }
            return false;
        }

        public string GetString(int ordinal)
        {
            if (fieldValues == null)
            {
                throw new Exception("Current line is null.");
            }
            return fieldValues[ordinal];
        }

        public DateTime GetDateTime(int ordinal)
        {
            if (fieldValues == null)
            {
                throw new Exception("Current line is null.");
            }
            return DateTime.Parse(fieldValues[ordinal]);
        }

        public Int64 GetInt64(int ordinal)
        {
            if (fieldValues == null)
            {
                throw new Exception("Current line is null.");
            }
            return Convert.ToInt64(fieldValues[ordinal]);
        }

        public Int32 GetInt32(int ordinal)
        {
            if (fieldValues == null)
            {
                throw new Exception("Current line is null.");
            }
            return Convert.ToInt32(fieldValues[ordinal]);
        }

        public void Dispose()
        {
            reader.Dispose();
        }
    }
}
