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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;

namespace AvePoint.GCommon.Utility
{
    public class AvePerformanceLogging
    {
        class LoggingEntry
        {
            public bool Enabled = false;
            public string LoggingTypeName { get; set; }
            public object EntryLock = new object();
            public List<object> LogObjects = new List<object>();
            public Thread OutputThread { get; set; }
        }

        static string outputDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        static int maxFileSize = 5 * 1024 * 1024;
        static int maxFileCount = 10;
        static object entriesLock = new object();
        static Dictionary<Type, LoggingEntry> entries = new Dictionary<Type, LoggingEntry>();
        static bool globalDisabled = true;

        public static void SetOutputDirectory(string directory)
        {
            outputDirectory = directory;
        }

        public static void SetMaxFileSize(int maxSize)
        {
            maxFileSize = maxSize;
        }

        public static void SetMaxFileCount(int maxCount)
        {
            maxFileCount = maxCount;
        }

        public static void EnableLogType(Type loggingObjectType, bool enabled)
        {
            globalDisabled = enabled ? false : globalDisabled;

            LoggingEntry loggingEntry = null;
            lock (entriesLock)
            {
                if (!entries.ContainsKey(loggingObjectType))
                {
                    loggingEntry = new LoggingEntry();
                    loggingEntry.LoggingTypeName = loggingObjectType.Name;
                    loggingEntry.OutputThread = new Thread(new ParameterizedThreadStart(OutputThread));
                    loggingEntry.OutputThread.IsBackground = true;
                    loggingEntry.OutputThread.Name = "Logging Monitor Thread: " + loggingObjectType.Name;
                    loggingEntry.OutputThread.Start(loggingEntry);
                    entries.Add(loggingObjectType, loggingEntry);
                }
                loggingEntry = entries[loggingObjectType];
            }
            loggingEntry.Enabled = enabled;
        }

        public static void DisableAllLogType()
        {
            globalDisabled = true;
        }

        public static void Log(object loggingObject)
        {
            if (globalDisabled) return;

            Type loggingType = loggingObject.GetType();
            LoggingEntry loggingEntry = null;
            lock (entriesLock)
            {
                if (!entries.ContainsKey(loggingType)) return;
                loggingEntry = entries[loggingType];
            }

            if (!loggingEntry.Enabled) return;
            lock (loggingEntry.EntryLock)
            {
                if (loggingEntry.LogObjects.Count < 10000)
                {
                    loggingEntry.LogObjects.Add(loggingObject);
                }
                else
                {
                    //log generated too fast, we can't write out
                }
            }
        }

        private static void OutputThread(object obj)
        {
            LoggingEntry loggingEntry = obj as LoggingEntry;

            while (true)
            {
                List<object> temp = new List<object>();
                lock (loggingEntry.EntryLock)
                {
                    temp.AddRange(loggingEntry.LogObjects);
                    loggingEntry.LogObjects.Clear();
                }
                if (temp.Count == 0)
                {
                    Thread.Sleep(3000);
                    continue;
                }

                string logFileFullPath = Path.Combine(outputDirectory, loggingEntry.LoggingTypeName + ".txt");
                StreamWriter sw = new StreamWriter(new FileStream(logFileFullPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite));
                int count = 1;
                bool checkOnce = false;
                foreach (var log in temp)
                {
                    sw.WriteLine(log.ToString());
                    if (count++ % 500 == 0 || !checkOnce)
                    {
                        checkOnce = true;
                        sw.Close();
                        sw = null;
                        RollingTheFile(logFileFullPath);
                        sw = new StreamWriter(new FileStream(logFileFullPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite));
                    }
                }
                if (sw != null)
                {
                    sw.Close();
                    sw = null;
                }
            }
        }

        private static void RollingTheFile(string logFileFullPath)
        {
            FileInfo fi = new FileInfo(logFileFullPath);
            if (fi.Length < maxFileSize) return;
            RenameFile(logFileFullPath, logFileFullPath, 1);
        }

        private static void RenameFile(string sourceFilePath, string logFileFullPath, int postFixNumber)
        {
            if (File.Exists(logFileFullPath + "." + postFixNumber))
            {
                RenameFile(logFileFullPath + "." + postFixNumber, logFileFullPath, postFixNumber + 1);
                File.Move(sourceFilePath, logFileFullPath + "." + postFixNumber);
            }
            else
            {
                if (postFixNumber - 1 == maxFileCount)
                {
                    File.Delete(logFileFullPath + "." + maxFileCount);
                }
                else
                {
                    File.Move(sourceFilePath, logFileFullPath + "." + postFixNumber);
                }
            }
        }

    }

    public class AvePerformanceLoggingTest
    {
        class MyLogObject1
        {
            public override string ToString()
            {
                return "A,B,C,D";
            }
        }

        class MyLogObject2
        {
            public override string ToString()
            {
                return "A B C D";
            }
        }

        static void T1()
        {
            while (true)
            {
                AvePerformanceLogging.Log(new MyLogObject1());
            }
        }

        static void T2()
        {
            while (true)
            {
                AvePerformanceLogging.Log(new MyLogObject2());
            }
        }

        static void T3()
        {
            while (true)
            {
                AvePerformanceLogging.Log("ABC");
            }
        }

        static void T4()
        {
            while (true)
            {
                AvePerformanceLogging.Log("DEF");
            }
        }

        public static void MainTest()
        {
            AvePerformanceLogging.SetOutputDirectory("C:\\");
            AvePerformanceLogging.SetMaxFileSize(5 * 1024);
            AvePerformanceLogging.SetMaxFileCount(5);
            AvePerformanceLogging.EnableLogType(typeof(MyLogObject1), true);
            AvePerformanceLogging.EnableLogType(typeof(MyLogObject2), true);
            AvePerformanceLogging.EnableLogType(typeof(string), true);
            //AvePerformanceLogging.DisableAllLogType();

            new Thread(T1).Start();
            new Thread(T2).Start();
            new Thread(T3).Start();
            new Thread(T4).Start();

            Thread.Sleep(int.MaxValue);
        }

    }

}
