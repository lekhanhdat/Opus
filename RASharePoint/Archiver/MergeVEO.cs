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
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.Item.Restore;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.RA.SharePoint.Common;
using AvePoint.StorageOptimization.Schedule.Common;
using AvePoint.Wrapper.Common;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.SharePoint.News.DataModel;
using RAExportCommon;
//using SP2013ComplianceVaultCommonUtility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace AvePoint.StorageOptimization.Schedule.Archiver
{
    public class MergeVEO
    {
        private IStorageDeviceService StorageDeviceService => PlatformWindsorManager.GetService<IStorageDeviceService>();
        public IExportSettingsDao ExportSettingsDao => (IExportSettingsDao)PlatformWindsorManager.GetService(typeof(IExportSettingsDao));


        private string JobId = string.Empty;
        public JobReportImps mJobreport;
        private JobContext jobContext = null;
        private static readonly AveLogger mLog = AveLogger.GetInstance(typeof(MergeVEO));
        public MergeVEO(string jobId)
        {
            JobId = jobId;
            jobContext = JobContext.GetInstance(jobId, JobType.VeoMerge);
            jobContext.ReportManager.StartUpdateJobProgress();
        }
        public void Merge()
        {
            int fileNumber = 0;
            double fileSize = 0;
            string folderName = string.Empty;
            bool isDeleteOldFile = false;
            try
            {
                var exportSetting = ExportSettingsDao.GetExportSetting((int)ExportSettingType.VEO, (int)SourceFlag.SharePoint);
                if (exportSetting != null)
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(exportSetting.ArchiverSetting);
                    isDeleteOldFile = bool.Parse(doc.SelectSingleNode("Configuration/archiverVEOMerge/isDeleteOldFile").InnerXml);
                    fileNumber = int.Parse(doc.SelectSingleNode("Configuration/archiverVEOMerge/fileNumber").InnerXml);
                    fileSize = double.Parse(doc.SelectSingleNode("Configuration/archiverVEOMerge/fileSize").InnerXml);
                    folderName = doc.SelectSingleNode("Configuration/archiverVEOMerge/folderName").InnerXml;
                }
                else
                {
                    var unZipFolder = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Config", "VEO Configuration Files");
                    using (FileStream fs = new FileStream(Path.Combine(unZipFolder, "ArchiverSettings.config"), FileMode.Open, FileAccess.Read))
                    {
                        using (StreamReader sr = new StreamReader(fs))
                        {
                            XmlDocument doc = new XmlDocument();
                            doc.LoadXml(sr.ReadToEnd());
                            isDeleteOldFile = bool.Parse(doc.SelectSingleNode("Configuration/archiverVEOMerge/isDeleteOldFile").InnerXml);
                            fileNumber = int.Parse(doc.SelectSingleNode("Configuration/archiverVEOMerge/fileNumber").InnerXml);
                            fileSize = double.Parse(doc.SelectSingleNode("Configuration/archiverVEOMerge/fileSize").InnerXml);
                            folderName = doc.SelectSingleNode("Configuration/archiverVEOMerge/folderName").InnerXml;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                mLog.Warn("set VEO export setting when run job error {0}", e.ToString());
            }
            List<string> jobIds = SerializerHelper.DeserializeByDataContractSerializer<List<string>>(jobContext.JobContextSetting);
            mJobreport = new JobReportImps(jobContext.ReportManager);
            var exportLocation = StorageDeviceService.GetExportDevice();
            if (exportLocation == null)
            {
                mLog.Error("Can not get Export Location.");
                if (mJobreport != null)
                {
                    mJobreport.HasErrorNode = true;
                    mJobreport.summaryComments = "RM_RDM_Rule_ConfigureExportLocation";
                    mJobreport.FinishRestoreReport();
                    return;
                }
                throw new Exception("RM_RDM_Rule_ConfigureExportLocation");
            }
            PhysicalDeviceDto device = ConvertStorageDeviceDtoToPhysicalDeviceDto(exportLocation);
            VaultMergeVEOFactory factory = new VaultMergeVEOFactory();
            mLog.Info(string.Format("MergeVEO property,FileNumber:{0}, FileSize:{1}, FolderName:{2}, IsDeleteOldFile{3}.", fileNumber, fileSize, folderName, isDeleteOldFile));
            mLog.Info(string.Format("MergeVEO jobID Collection Count:{0}.", jobIds.Count));
            foreach (string jobId in jobIds)
            {
                List<MergeVEOJobDetail> jobDetails = new List<MergeVEOJobDetail>();
                try
                {
                    using (new CheckJobStopScope())
                    {
                        IVaultMergeVEO mergeVEO = factory.Create(VaultMergeType.Base, fileNumber, fileSize, ReplaceSpecicalCharactersToUnderline(folderName), isDeleteOldFile);
                        mLog.Info(string.Format("MergeVEO Current JobID:{0}, Device Path:{1}.", jobId, device.Name));
                        List<MergeVEOJobDetail> jobDetail = mergeVEO.MergeVEO(device, jobId);
                        jobDetails = jobDetails.Concat(jobDetail).ToList();
                        foreach (var detail in jobDetails)
                        {
                            mJobreport.AddVEOMergeReport(detail.FileName, detail.SourceFolder, detail.DesFolder, detail.Status, detail.Size, detail.FinishTime);
                        }
                        //mConfiguration.MergeVEOReportDto.AddMergeVEOReport(jobDetail, jobId);
                        mJobreport.UpdateProgress(true);
                    }
                }
                catch (JobStopException ex)
                {
                    mLog.Warn("job is stopped");
                    mJobreport.HasStop = true;
                    if (mJobreport != null)
                    {
                        mJobreport.FinishRestoreReport();
                    }
                    throw;
                }
                catch (Exception ex)
                {
                    mLog.Error("An Error Occur while MergeVEO, Current JobID:{0}, Device Path:{1},Message:{2}.", jobId, device.Name, ex.ToString());
                    mJobreport.HasErrorNode = true;
                }
                //同一个Job设置多个rule，每个rule设置不同的Export Location，应该在所有Location都Merge完再更新Job状态.
                ProcessJobStatus(jobDetails);
            }
            if (mJobreport != null)
            {
                mJobreport.FinishRestoreReport();
            }
        }
        private PhysicalDeviceDto ConvertStorageDeviceDtoToPhysicalDeviceDto(StorageDeviceDto storageDevice)
        {
            var physical = new PhysicalDeviceDto()
            {
                Id = storageDevice.Id,
                ConnectionString = storageDevice.ConnectionString,
                ModifyTime = storageDevice.ModifyTime,
                Type = storageDevice.Type,
            };
            return physical;
        }
        /// <summary>
        /// 替换Folder Name中的特殊字符
        /// </summary>
        /// <returns></returns>
        private string ReplaceSpecicalCharactersToUnderline(string folderName)
        {
            string returnFolderName = string.Empty;
            try
            {
                string reg = @"\:" + @"|\/" + @"|\\" + @"|\|" + @"|\*" + @"|\?" + @"|\""" + @"|\<" + @"|\>";//特殊字符
                Regex r = new Regex(reg);
                returnFolderName = r.Replace(folderName, "_");
                mLog.Info("Replace Special Characters To Underline while Merge VEO job, SourceFolderName:{0}, ConvertFolderName:{1}.", folderName, returnFolderName);
            }
            catch (Exception ex)
            {
                returnFolderName = folderName;
                mLog.Warn("Can not Replace Special Characters To Underline while Merge VEO job, Message:{0}.", ex.ToString());
            }
            return returnFolderName;
        }

        private void ProcessJobStatus(List<MergeVEOJobDetail> jobDetail)
        {
            try
            {
                //有失败的，有成功的，job Exception.
                if (jobDetail.Where(t => t.Status == 1).Count() != 0 && jobDetail.Where(t => t.Status == 0).Count() != 0)
                {
                    mJobreport.HasErrorNode = true;
                    mJobreport.HasCompleteNode = true;
                }
                //只有失败的，没有成功的，job failed.
                else if (jobDetail.Where(t => t.Status == 1).Count() != 0 && jobDetail.Where(t => t.Status == 0).Count() == 0)
                {
                    mJobreport.HasErrorNode = true;
                    mJobreport.HasCompleteNode = false;
                }
                //只有成功的，没有失败的，jobfinished.
                else if (jobDetail.Where(t => t.Status == 1).Count() == 0 && jobDetail.Where(t => t.Status == 0).Count() != 0)
                {
                    mJobreport.HasErrorNode = false;
                    mJobreport.HasCompleteNode = true;
                }
            }
            catch (Exception e)
            {
                mLog.Info(string.Format("Can not process merge VEO job status, Message:{0}.", e.ToString()));
            }
        }
    }
}
