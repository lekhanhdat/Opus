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
using System.Threading.Tasks;
using AvePoint.Adonis.Records.Object;
using AvePoint.Adonis.Records.Object.SP;
using System.Data.SQLite;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.Media.Storage;
using AvePoint.Records.Core;
using AvePoint.Records.Core.JobProgress;
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.GCommon;
using AvePoint.Adonis.Records.Object.Report;
using System.IO;

namespace AvePoint.RA.Service.Services.RMReport
{
    public class MergeReportFileWorker
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(MergeReportFileWorker));
        private RecordsJobMessage message;
        IProgressService progressServcie;

        public MergeReportFileWorker(RecordsJobMessage message)
        {
            this.message = message;
            JobContext.Current.Init(message);
            progressServcie = JobContext.Current.ProgressManager.Create();

        }

        public void Run()
        {
            var reportMessage = (message as ReportJobMessage).ReportMessage;
            var jobMessage = message as ReportJobMessage;
            try
            {
                var location = reportMessage.ReportLocation;
                string mainJobId = message.Job.Id.Split('_')[0];
                var jobPath = AssembleReportPathAfterHalf(mainJobId, jobMessage.MergeReportRealType, ".rpt");
                string dest = System.IO.Path.Combine(location.Location, jobPath);

                logger.Info("start merge job {0} : {1}", mainJobId, dest);
                string[] files = GetAllFiles(location, mainJobId);
                if (files.Length > 0)
                {
                    progressServcie.IncreaseBase(files.Length);
                    var userPassWord = CryptoUtil.ConvertBytesToString(CspCommunicationWrapper.UnWrapKey(reportMessage.LocationAccount.Password));
                    using (AveImpersonator impersonator = new AveImpersonator(reportMessage.LocationAccount.UserName, userPassWord))
                    {
                        impersonator.Impersonate();
                        logger.Info("Start to merge the rpt files.");

                        Merge(files, dest);
                        logger.Info("Start to delete the rpt files created by the subjobs.");
                        DeleteTempFiles(files);
                    }
                }
                else
                {
                    logger.Info("Current Job's report file is Empty {0}", mainJobId);
                }
                JobContext.Current.JobSummaryService.NotifyManager(AvePoint.Common.JobState.Finished);
            }
            catch (Exception ex)
            {
                logger.Error("Merge job failed {0}", ex.ToString());
                JobContext.Current.JobSummaryService.NotifyManager(AvePoint.Common.JobState.Failed, ex.Message);
            }


        }

        private void DeleteTempFiles(string[] files)
        {
            if (files.Length == 0) return;
            try
            {
                foreach (var file in files)
                {
                    try
                    {
                        System.IO.File.Delete(file);
                    }
                    catch (Exception ex)
                    {
                        logger.Warn("Failed to delete the file:{0}  Exception:{1}", file, ex.ToString());
                    }
                }

                string folder = System.IO.Path.GetDirectoryName(files[0]);
                System.IO.Directory.Delete(folder, true);
            }
            catch (Exception ex1)
            {
                logger.Warn("Failed to delete the folder.  Exception:{0}", ex1.ToString());
            }
        }
        private static string AssembleReportPathAfterHalf(string jobId, RMMessageType jobType, string expandedName)
        {
            StringBuilder stringBuild = new StringBuilder();
            string moduleName = string.Empty;
            switch (jobType)
            {
                case RMMessageType.CreateAndDestroyedFileReport:
                    moduleName = "Content Due for Time Frame Report";
                    break;
                case RMMessageType.BCSTermUsageReport:
                    moduleName = "Term Usage Report";
                    break;
                case RMMessageType.ItemsFilesDueDisposal:
                    moduleName = "Content Due for Disposal Report";
                    break;
                case RMMessageType.AvailableSpaceReport:
                    moduleName = "Available Space Report";
                    break;
                default:
                    moduleName = "Default";
                    break;
            }
            stringBuild.Append("RAScheduleJob");
            stringBuild.Append("\\");
            stringBuild.Append(moduleName);
            stringBuild.Append("\\");
            stringBuild.Append(jobId);
            stringBuild.Append(expandedName);

            return stringBuild.ToString();
        }
        private string[] GetAllFiles(PhysicalDeviceDto location, string id)
        {//TODO  improvement:  make the file with the max size to be the fist one.
            List<string> rptFiles = new List<string>();
            using (IXSystem system = XFactory.InstanceSystem(location.ConnectionString))
            {
                StorageOpenValidResult rs = system.Validate();
                var folder = new StorageInfo() { HighName = id, LowName = "" };
                try
                {
                    var files = system.ListFiles(folder);
                    foreach (var file in files)
                    {
                        rptFiles.Add(file.FileFullPath);
                    }
                }
                catch (Exception e)
                {
                    logger.Info("Get all files failed {0}", e.ToString());
                }

            }
            return rptFiles.ToArray();
        }

        private void Merge(string[] files, string dest)
        {
            FileInfo info = new FileInfo(dest);
            if (!info.Directory.Exists)
            {
                info.Directory.Create();
            }
            System.IO.File.Copy(files[0], dest, true);
            logger.Info("Copy ...{0}", files[0]);
            progressServcie.Increase();
            if (files.Length > 1)
            {
                SQLiteConnectionStringBuilder builder = new SQLiteConnectionStringBuilder();
                if (dest.StartsWith("\\\\"))
                {
                    builder.DataSource = "\\\\" + dest;
                }
                else
                {
                    builder.DataSource = dest;
                }


                using (SQLiteConnection conn = new SQLiteConnection(builder.ToString()))
                {
                    conn.Open();
                    string insertCmdTxt = BuildInsertCmdText(conn);

                    using (SQLiteCommand cmd = conn.CreateCommand())
                    {
                        for (int i = 1; i < files.Length; i++)
                        {
                            logger.Info("Copy ...{0}", files[i]);
                            cmd.CommandText = string.Format("ATTACH '{0}' AS TOMERGE", files[i]);
                            cmd.ExecuteNonQuery();
                            cmd.CommandText = insertCmdTxt;
                            cmd.ExecuteNonQuery();
                            cmd.CommandText = string.Format("DETACH DATABASE TOMERGE");
                            cmd.ExecuteNonQuery();
                            progressServcie.Increase();
                        }
                    }
                }
            }
        }
        private static string BuildInsertCmdText(SQLiteConnection conn)
        {
            List<string> names = new List<string>();
            using (SQLiteCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = "PRAGMA table_info(ReportDetail)";
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (!reader.GetBoolean(5))
                        {
                            names.Add(reader.GetString(1));
                        }
                    }
                }
            }
            var namesStr = string.Join(",", names);
            StringBuilder cmdBuilder = new StringBuilder("INSERT INTO ReportDetail ")
                .Append(" (").Append(namesStr).Append(") ")
                .Append(" SELECT ").Append(namesStr).Append(" FROM TOMERGE.ReportDetail");
            return cmdBuilder.ToString();
        }
    }
}
