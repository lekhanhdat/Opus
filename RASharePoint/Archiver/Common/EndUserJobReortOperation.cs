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
using System.IO;
using System.Diagnostics;
using System;
using System.Collections;
using AvePoint.GCommon.Contract.Server.Job.Object;
using AvePoint.GCommon;
using System.Reflection;
using System.Collections.Generic;
using AvePoint.Common;
using AvePoint.Wrapper.Common;
using System.Threading;
using System.Linq;

namespace AvePoint.RA.SharePoint.Archiver
{
    public class RelativeDataJobReortOperation : IDisposable
    {
        private static readonly AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private string reportDBFile = string.Empty;
        private string scheduleDir = string.Empty;
        private readonly object mlock = new object();

        public RelativeDataJobReortOperation(string jobID)
        {
            InitField(jobID);
            if (File.Exists(reportDBFile))
            {
                //File.Delete(reportDBFile);
            }
            CreateDBFile();
        }

        private void InitField(string jobID)
        {
            scheduleDir = Path.Combine(AveEnv.AgentJobFolder, jobID);
            reportDBFile = Path.Combine(scheduleDir, jobID + "_Report.txt");
        }


        private void CreateDBFile()
        {
            if (File.Exists(reportDBFile))
            {
                return;
            }
            if (!Directory.Exists(scheduleDir))
            {
                Directory.CreateDirectory(scheduleDir);
            }
        }

        public List<JobDetail> GetReports()
        {
            List<JobDetail> jobDetails = new List<JobDetail>();
            try
            {
                lock (mlock)
                {
                    using (FileStream fs = new FileStream(reportDBFile, FileMode.Open, FileAccess.Read))
                    {
                        StreamReader sr = new StreamReader(fs);
                        string details = string.Empty;
                        while ((details = sr.ReadLine()) != null)
                        {
                            string[] splitDetail = details.Split(';');
                            JobDetail detail = new JobDetail()
                            {
                                SubJobId = splitDetail[0],
                                Type = splitDetail[1],
                                SrcURL = splitDetail[2],
                                Size = Convert.ToInt64(splitDetail[3]),
                                Status = Convert.ToInt32(splitDetail[4]),
                                Remark12 = splitDetail[5],
                                Message = splitDetail[6]
                            };
                            jobDetails.Add(detail);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mLog.Warn("Can not get reports from temp file,FileName:{0},Message:{1}.", reportDBFile, ex.ToString());
            }
            return jobDetails;
        }

        public void Close()
        {

        }

        public void Dispose()
        {
            Close();
        }

        public void AddDetail(JobDetail jobDetail)
        {
            try
            {
                lock (mlock)
                {
                    using (FileStream fst = new FileStream(reportDBFile, FileMode.Append, FileAccess.Write))
                    {
                        StreamWriter sw = new StreamWriter(fst);
                        string detail = jobDetail.SubJobId + ";" + jobDetail.Type + ";" + jobDetail.SrcURL + ";" + jobDetail.Size + ";" + jobDetail.Status + ";" + jobDetail.Remark12 + ";" + jobDetail.Message;
                        sw.WriteLine(detail);
                        sw.Flush();
                    }
                }
            }
            catch (Exception ex)
            {
                mLog.Error("Error in Insert into Report DB" + ex.ToString());
            }
        }
    }
}