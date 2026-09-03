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



namespace AvePoint.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Diagnostics;
    using AvePoint.GCommon.Utility;
    using System.IO;
    using System.Xml;
    using AvePoint.GCommon;

    /// <summary>
    /// <Activities>
    ///     <Jobs>
    ///         <Job jobId="XXX" pid="123" />
    ///         <Job jobId="XXX" pid="123" />
    ///     </Jobs>
    ///     <Processes>
    ///         <Process name="XXX" />
    ///         <Process name="XXX" />
    ///     </Processes>
    /// </Activities>
    /// </summary>
    public class GlobalActivity
    {
        static AveLogger log = AveLogger.GetInstance(typeof(GlobalActivity));
        static readonly string activityFileName = "Activities.dat";
        static readonly string mutexLockName = "GlobalActivityOperateFileLockMutex";

        public static void RegisterJob(string jobId)
        {
            RegisterJob(jobId, Process.GetCurrentProcess().Id);
        }

        public static void RegisterJob(string jobId, int pid)
        {
            using (AveMutex mutex = new AveMutex(mutexLockName, false))
            {
                try
                {
                    mutex.WaitLocked();
                    string activities = LoadActivityFile();
                    XmlDocument xDoc = new XmlDocument();
                    xDoc.LoadXml(activities);
                    XmlElement jobs = (XmlElement)xDoc.GetElementsByTagName("Jobs")[0];
                    XmlElement job = xDoc.CreateElement("Job");
                    job.SetAttribute("jobId", jobId);
                    job.SetAttribute("pid", pid.ToString());
                    jobs.AppendChild(job);
                    SaveActivityFile(xDoc.DocumentElement.OuterXml);
                }
                catch (Exception e)
                {
                    log.Error("Register job:{0},pid:{1} failed,we will delete the register file,error:{2}", jobId, pid, e.ToString());
                    DeleteActivityFile();
                }
                finally
                {
                    mutex.ReleaseLock();
                }
            }
        }

        public static void UnRegisterJob(string jobID)
        {
            UnRegisterJob(jobID, Process.GetCurrentProcess().Id);
        }

        public static void UnRegisterJob(string jobID, int pid)
        {
            using (AveMutex mutex = new AveMutex(mutexLockName, false))
            {
                try
                {
                    mutex.WaitLocked();

                    string activities = LoadActivityFile();
                    XmlDocument xDoc = new XmlDocument();
                    xDoc.LoadXml(activities);
                    XmlElement jobs = (XmlElement)xDoc.GetElementsByTagName("Jobs")[0];
                    XmlElement removedJob = null;
                    foreach (XmlElement job in jobs)
                    {
                        if (string.Compare(job.GetAttribute("jobId"), jobID, StringComparison.OrdinalIgnoreCase) == 0
                            && string.Compare(job.GetAttribute("pid"), pid.ToString(), StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            removedJob = job;
                            break;
                        }
                    }
                    if (removedJob != null)
                    {
                        jobs.RemoveChild(removedJob);
                    }

                    SaveActivityFile(xDoc.DocumentElement.OuterXml);
                }
                catch (Exception e)
                {
                    log.Error("Unregister job:{0},pid:{1} failed,we will delete the register file,error:{2}", jobID, pid, e.ToString());
                    DeleteActivityFile();
                }
                finally
                {
                    mutex.ReleaseLock();
                }
            }

        }

        public static string GetAllActivities()
        {
            string activities = LoadActivityFile();
            //deal with activity jobs
            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(activities);
            List<XmlElement> deadJobs = new List<XmlElement>();
            foreach (XmlElement xEl in xDoc.GetElementsByTagName("Job"))
            {
                try
                {
                    int pid = int.Parse(xEl.GetAttribute("pid"));
                    Process.GetProcessById(pid);
                }
                catch (System.ArgumentException)
                {
                    deadJobs.Add(xEl);
                }
            }
            XmlElement jobs = (XmlElement)xDoc.GetElementsByTagName("Jobs")[0];
            foreach (XmlElement deadJob in deadJobs)
            {
                jobs.RemoveChild(deadJob);
            }

            //deal with running process
            XmlElement processes = (XmlElement)xDoc.GetElementsByTagName("Processes")[0];
            List<string> processNames = new List<string>();
            string[] files = Directory.GetFiles(AveEnv.AgentBinFolder, "*.exe");
            foreach (string file in files)
            {
                FileInfo fi = new FileInfo(file);
                if (string.Compare(fi.Name, AgentConstants.AgentBinaryName.POSTINSTALL_EXE_NAME, StringComparison.OrdinalIgnoreCase) == 0
                    || string.Compare(fi.Name, AgentConstants.AgentBinaryName.SERVICE_EXE_NAME, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    continue;
                }
                string processName = fi.Name.Substring(0, fi.Name.Length - 4);
                Process[] ps = Process.GetProcessesByName(processName);
                if (ps.Length > 0)
                {
                    XmlElement process = xDoc.CreateElement("Process");
                    process.SetAttribute("name", processName);
                    processes.AppendChild(process);
                }
            }

            return xDoc.DocumentElement.OuterXml;
        }

        private static string LoadActivityFile()
        {
            string activityFilePath = Path.Combine(AveEnv.AgentDataFolder, activityFileName);
            if (File.Exists(activityFilePath))
            {
                string activityFileContent = File.ReadAllText(activityFilePath, Encoding.UTF8);
                if (string.IsNullOrEmpty(activityFileContent))
                {
                    return CreateActivityFileContentModel();
                }
                return activityFileContent;
            }
            else
            {
                return CreateActivityFileContentModel();
            }
        }

        private static string CreateActivityFileContentModel()
        {
            XmlDocument xDoc = new XmlDocument();
            XmlElement activities = xDoc.CreateElement("Activities");
            XmlElement jobs = xDoc.CreateElement("Jobs");
            XmlElement processes = xDoc.CreateElement("Processes");
            activities.AppendChild(jobs);
            activities.AppendChild(processes);
            return activities.OuterXml;
        }

        private static void SaveActivityFile(string activityXml)
        {
            string activityFilePath = Path.Combine(AveEnv.AgentDataFolder, activityFileName);
            File.WriteAllText(activityFilePath, activityXml, Encoding.UTF8);
        }

        private static void DeleteActivityFile()
        {
            string activityFilePath = Path.Combine(AveEnv.AgentDataFolder, activityFileName);
            try
            {
                if (File.Exists(activityFilePath))
                {
                    File.Delete(activityFilePath);
                }
            }
            catch (Exception e)
            {
                log.Warn("Delete activity file:{0} failed:{1}", activityFilePath, e.ToString());
            }
        }
    }
}
