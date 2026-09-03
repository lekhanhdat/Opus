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

using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Global.Util;
using AvePoint.RA.Common.Util;
using log4net;
using log4net.Repository.Hierarchy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace AvePoint.RA.CommonUtil
{
    internal class LogUploadService
    {
        private LogUploadThread LogUploadThread { get; set; }

        private static LogUploadService _instance;

        private RALogger logger = RALogger.GetInstance(typeof(LogUploadService));

        public bool HasLogNeedUpload
        {
            get
            {
                return LogUploadThread.HasLogNeedUpload;
            }
        }

        private LogUploadService()
        {
            Start();
        }

        private static readonly object lockObj = new object();//Quality Issue
        public static LogUploadService GetInstance()
        {
            lock (lockObj)
            {
                if (_instance == null)
                {
                    _instance = new LogUploadService();
                }
            }
            return _instance;
        }

        public static void SetLogType(LogType mLogType)
        {
            lock (lockObj)
            {
                if (_instance == null)
                {
                    _instance = new LogUploadService();
                }
                _instance.LogUploadThread.UploadLogTye = mLogType;
            }
        }

        public void ApplyXriString(string xriString, string containerName)
        {
            LogUploadThread.XriString = xriString;
            LogUploadThread.ContainerName = containerName;
        }

        public void UploadLog(string fileName)
        {
            try
            {
                LogUploadThread.UploadLog(fileName);
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while uploading log file.", ex);
            }
        }

       /* private void Restart()
        {
            Stop();
            Start();
        }*/

 

        private void Start()
        {
            LogUploadThread = new LogUploadThread();
            LogUploadThread.UploadLogTye = LogType.ServiceLog;
            //Thread thread = new Thread(LogUploadThread.DoUpload);
            //thread.IsBackground = true;
            //thread.Start();
        }


    }

    public class LogUploadThread
    {
        internal LogType UploadLogTye;

        private Queue<string> logFileQueue = new Queue<string>();

        private bool Interrupted { get; set; }

        private string xriString;

        public string XriString
        {
            set
            {
                lock (configurationLock)
                {
                    xriString = value;
                    Monitor.PulseAll(configurationLock);
                }
            }
        }

        private string containerName;

        public string ContainerName
        {
            set
            {
                lock (configurationLock)
                {
                    containerName = value;
                    Monitor.PulseAll(configurationLock);
                }
            }
        }

        private readonly object configurationLock = new object();



        public bool HasLogNeedUpload = true;

        public void UploadLog(string fileName)
        {
            lock (logFileQueue)
            {
                logFileQueue.Enqueue(fileName);
                this.HasLogNeedUpload = true;
                Monitor.PulseAll(logFileQueue);
            }
        }

        public void Stop()
        {
            Interrupted = true;
        }

    }

}
