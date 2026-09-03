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
using AvePoint.Common;
using AvePoint.GCommon;
using AvePoint.GCommon.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.Archiver
{
    public class CallProcess
    {
        private AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private Random random = new Random();
        private long interval = 30 * 1000 * 10000;//cache ids for 30s

        private List<string> randomIds = new List<string>();
        private readonly object locker = new object();
        private long lastTime = 0;
        private long now = DateTime.Now.Ticks;
        //public void WriteArchiveMsgToLocal(string folderPath, string fileName, object msg)
        //{
        //    if(!Directory.Exists(folderPath))
        //    {
        //        Directory.CreateDirectory(folderPath);
        //    }
        //    string msgString = SerializerHelper.SerializeByDataContractSerializer(msg);
        //    using (StreamWriter sw = new StreamWriter(Path.Combine(folderPath, fileName), false))
        //    {
        //        sw.Write(msgString);
        //    }
        //}

        //public void StartSOMessageCenterProcess(string jobType, string archiveMessagePath, out string jobId)
        //{
        //    jobId = string.Empty;
        //    StartSOMessageCenterProcess(jobType, archiveMessagePath);
        //    string jobIdStartString = string.Empty;
        //    string jobIdEndString = string.Empty;
        //    switch (jobType)
        //    {
        //        case ArchiveConstants.EndUserJob:
        //            {
        //                jobIdStartString = "EA";
        //                jobIdEndString = "A0";
        //                break;
        //            }
        //        case ArchiveConstants.MergeIndexJob:
        //            {
        //                jobIdStartString = "EA";
        //                jobIdEndString = "M0";
        //                break;
        //            }
        //        default:
        //            {
        //                break;
        //            }
        //    }
        //    jobId = jobIdStartString + DateTime.Now.ToString(AveDateTimeUtility.DATETYPE017, DateTimeFormatInfo.InvariantInfo) + GenerateRandomId() + jobIdEndString;
        //}

        //public void StartSOMessageCenterProcess(string jobType, string archiveMessagePath)
        //{
        //    mLog.Info(string.Format("begin to start: {0}, JobType is: {1}, Temp file path is: {2} ", AgentConstants.AgentBinaryName.SO_MessageCenter, jobType, archiveMessagePath));
        //    StartProcess sp = new StartProcess(AveEnv.AgentBinFolder);
        //    try
        //    {
        //        Process process = sp.Start(Path.Combine(AveEnv.AgentBinFolder, AgentConstants.AgentBinaryName.SO_MessageCenter), jobType + " " + archiveMessagePath);
        //        if (process.HasExited)
        //        {
        //            throw new ArgumentException("Process is not running.");
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        mLog.Log(AveLogLevel.ERROR, e.ToString());
        //        throw new Exception("Cannot start process");
        //    }
        //    mLog.Info(AgentConstants.AgentBinaryName.SO_MessageCenter + " is running.");
        //}

        public string GenerateJobId(string jobType)
        {
            string jobId = string.Empty;
            string jobIdStartString = string.Empty;
            string jobIdEndString = string.Empty;
            switch (jobType)
            {
                case ArchiveConstants.RelativeDataJob:
                    {
                        jobIdStartString = "EA";
                        jobIdEndString = "A0";
                        break;
                    }
                case ArchiveConstants.MergeIndexJob:
                    {
                        jobIdStartString = "EA";
                        jobIdEndString = "M0";
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
            jobId = jobIdStartString + DateTime.Now.ToString(AveDateTimeUtility.DATETYPE017, DateTimeFormatInfo.InvariantInfo) + GenerateRandomId() + jobIdEndString;
            return jobId;
        }

        public string GeneratePlanId()
        {
            string planId = string.Empty;
            planId = DateTime.Now.ToString(AveDateTimeUtility.DATETYPE017, DateTimeFormatInfo.InvariantInfo) + GenerateRandomId();
            return planId;
        }

        private string GenerateRandomId()
        {   /* Fortify Issue Type: Insecure Randomness 
			 * Sink Details: this calss   92 139 146 
			 * Ignore Reason: random用于生成jobid，不影响安全性
			*/
            string randomNum = random.Next(1000000).ToString("000000");
            lock (locker)
            {
                try
                {
                    while (randomIds.Contains(randomNum))
                    {
                        randomNum = random.Next(1000000).ToString("000000");
                    }
                    if (((lastTime - now) > interval) && (randomIds.Count > 0))
                    {
                        randomIds.Clear();
                        now = DateTime.Now.Ticks;
                    }
                    randomIds.Add(randomNum);
                }
                catch (Exception ex)
                {
                    mLog.Info("Error occur while GenerateRandomId.Message:{0}.", ex.ToString());
                    randomNum = random.Next(1000000).ToString("000000");
                }
            }
            return randomNum;
        }
    }
}
