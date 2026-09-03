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
using AvePoint.GCommon.Contract.Server.Common.BackupDataSearch;
using AvePoint.GCommon.Contract.Server.Job;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RestoreCenter;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.ArchiverRestore;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.SharePoint.Discover;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Office2016.Drawing.Charts;
using DocumentFormat.OpenXml.Packaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.SiteCollectionMapping
{
    public class ImportSiteCollectionMappingProcessor
    {
        protected static readonly IRALogger mLog = RALogger.GetInstance(typeof(ImportTermProcessor));

        string mJobId;
        string mPath;
        long mFailedCount;
        long mSucceedCount;
        string errorMessage;
        private IRMRestoreSiteMappingDao RMRestoreSiteMappingDao => PlatformWindsorManager.GetService<IRMRestoreSiteMappingDao>();
        private ISettingProfilesDao settingProfilesDao => PlatformWindsorManager.GetService<ISettingProfilesDao>();

        private IRestoreSearchService restoreSearchService;
        private IRestoreSearchService RestoreSearchService => PlatformWindsorManager.GetService(ref restoreSearchService);

        private IJobMonitorService jobMonitorService;
        private IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService(ref jobMonitorService);

        public ImportSiteCollectionMappingProcessor(string jobId, string path)
        {
            mJobId = jobId;
            mPath = path;
            JobMonitorService.UpdateJobProgress(mJobId, 10);
        }

        public void RunJob()
        {
            string filePath = null;
            try
            {
                using (CheckJobStopScope jScope = new CheckJobStopScope())
                {
                    mLog.Info("Start to import");

                    filePath = DownloadFile();

                    List<SiteMappingInfo> excelData = ParseExcelAsync(filePath);

                    #region check Data
                    if(excelData.Count == 0)
                    {
                        errorMessage = "RM_RS_SiteMappings_EmptyExcel";
                        return;
                    }

                    foreach (SiteMappingInfo siteMapping in excelData)
                    {
                        siteMapping.SourceSiteUrl = siteMapping.SourceSiteUrl?.TrimEnd('/')?.TrimEnd('\\');
                        siteMapping.TargetSiteUrl = siteMapping.TargetSiteUrl?.TrimEnd('/')?.TrimEnd('\\');
                    }

                    RestoreSearchService.CheckSCMappings(excelData, out List<SiteMappingInfo> targetNotExistData, out List<SiteMappingInfo> notSameSourceData, out List<SiteMappingInfo> unKnowExceptionData, out List<SiteMappingInfo> validData, out Dictionary<string, List<SiteMappingInfo>> dedupData);
                    if(dedupData.Count > 0)
                    {
                        errorMessage = "RM_RS_SiteMappings_HaveDedupSourceUrl";
                        return;
                    }
                    mFailedCount += notSameSourceData.Count + targetNotExistData.Count + unKnowExceptionData.Count;

                    foreach(SiteMappingInfo info in notSameSourceData) 
                    {
                        mLog.Warn($"not same source data: {info.SourceSiteUrl} ： {info.TargetSiteUrl}");
                    }
                    foreach (SiteMappingInfo info in targetNotExistData)
                    {
                        mLog.Warn($"target Not Exist Data: {info.SourceSiteUrl} ： {info.TargetSiteUrl}");
                    }
                    foreach (SiteMappingInfo info in unKnowExceptionData)
                    {
                        mLog.Warn($"un Know Exception Data: {info.SourceSiteUrl} ： {info.TargetSiteUrl}");
                    }

                    JobMonitorService.UpdateJobProgress(mJobId, 30);
                    #endregion

                    var getIsOverrideSettingProfile = settingProfilesDao.LoadByType((int)SettingProfilesType.ImportSiteMappingOverrideInfo);


                    if (getIsOverrideSettingProfile != null && getIsOverrideSettingProfile.Settings != null && getIsOverrideSettingProfile.Settings.Equals("true"))
                    {
                        mLog.Info("The IsOverride is checked.");
                        DeleteAllDataForOverwrite();
                        SaveDataInDB(excelData);
                    }
                    else
                    {
                        mLog.Info("The IsOverride is not checked.");
                        DeleteConflictData(validData);
                        SaveDataInDB(validData);
                    }

                    mLog.Info("Finish to import");
                }
            }
            catch (JobStopException)
            {
                mLog.Info("Import is stopped.");
            }
            catch (TermCsvFormateExcetion te)
            {
                errorMessage = "RM_JS_JM_ImportFileFormatError";
                mLog.Error("The xlsx file is error :{0}", te.ToString());
            }
            catch (Exception e)
            {
                errorMessage = "RM_RS_SiteMappings_ErrorMessage";
                mLog.Error("Some error occurred.Error:{0}", e.ToString());
            }
            finally
            {
                mLog.Info("SucceedCount:[{0}] FailedCount:[{1}]", mSucceedCount, mFailedCount);
                UpdateJobStatus();
                DeleteFile(filePath);
            }
        }
        public void DeleteAllDataForOverwrite()
        {
            CheckJobIsStoping();
            RMRestoreSiteMappingDao.DeleteAllMappingByPage();
            JobMonitorService.UpdateJobProgress(mJobId, 50);
        }

        private void DeleteFile(string filePath)
        {
            File.Delete(filePath);
        }

        public string DownloadFile()
        {
            mLog.Info($"Path InvalidFileNameChars: {string.Join(",", Path.GetInvalidFileNameChars())}");
            mLog.Info($"Path InvalidPathChars: {string.Join(",", Path.GetInvalidPathChars())}");
            if (!mPath.EndsWith(".xlsx"))
            {
                errorMessage = "RM_JS_JM_ImportFileFormatError";
                throw new Exception();
            }

            CheckJobIsStoping();

            try
            {
                return JobReportUtility.GetImportJobCSVFile(mPath);
            }
            catch (Exception e)
            {
                mLog.Error("can not download file:{0}, error:{1}", mPath, e.ToString());
                throw;
            }
        }


        public List<SiteMappingInfo> ParseExcelAsync(string filePath)
        {
            CheckJobIsStoping();
            Dictionary<string, List<string[]>> datas = new Dictionary<string, List<string[]>>();
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    datas = ExcelUtil.ReadExcel(fs, null);
                }
            }
            catch (OpenXmlPackageException e)
            {
                if (e.ToString().Contains("Invalid Hyperlink") || e.ToString().Contains("Invalid URI"))
                {
                    using (FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                    {
                        UriFixer.FixInvalidUri(fs, brokenUri => UriFixer.FixUri(brokenUri));
                    }
                    using (FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                    {
                        datas = ExcelUtil.ReadExcel(fs);
                    }
                }
            }

            List<SiteMappingInfo> res = new List<SiteMappingInfo>();
            foreach (List<string[]> shell in datas.Values)
            {
                if(shell == null)
                {
                    continue;
                }
                for (int rowIndex = 0; rowIndex < shell.Count; rowIndex++)
                {
                    string[] row = shell[rowIndex];
                    if (row.Length >= 2 && !string.IsNullOrWhiteSpace(row[0]) && !string.IsNullOrWhiteSpace(row[1]))
                    {
                        res.Add(new SiteMappingInfo
                        {
                            SourceSiteUrl = row[0].Trim('/'),
                            TargetSiteUrl = row[1].Trim('/')
                        });
                    }
                }
            }

            JobMonitorService.UpdateJobProgress(mJobId, 20);
            CheckJobIsStoping();
            return res;
        }

        public void DeleteConflictData(List<SiteMappingInfo> sources)
        {
            CheckJobIsStoping();
            HashSet<string> willAddDataSources = sources.Select(info => info.SourceSiteUrl).ToHashSet();
            List<string> conflictSourceId = new List<string>();
            List<RMRestoreSiteMapping> existsMapping = RMRestoreSiteMappingDao.GetAllMappings();
            foreach (RMRestoreSiteMapping siteMappingInfo in existsMapping)
            {
                if (willAddDataSources.Contains(siteMappingInfo.SourceSiteUrl))
                {
                    conflictSourceId.Add(siteMappingInfo.Id);
                }
            }
            DatabaseUtility.BatchOperation(conflictSourceId, batchs =>
            {
                RMRestoreSiteMappingDao.BatchDeleteMapping(batchs.ToArray());
            });
            JobMonitorService.UpdateJobProgress(mJobId, 50);
        }

        public void SaveDataInDB(List<SiteMappingInfo> sources)
        {
            CheckJobIsStoping();
            int maxIntId = RMRestoreSiteMappingDao.GetLastMappingIntId();
            List<RMRestoreSiteMapping> siteMappings = new List<RMRestoreSiteMapping>(1500);
            for(int i = 0; i < sources.Count;)
            {
                SiteMappingInfo siteMappingInfo = sources[i];
                siteMappings.Add(new RMRestoreSiteMapping
                {
                    Id = Guid.NewGuid().ToString(),
                    SourceSiteUrl = siteMappingInfo.SourceSiteUrl,
                    TargetSiteUrl = siteMappingInfo.TargetSiteUrl,
                    intId = ++maxIntId,
                    SettingFlag = RestoreSettingFlag.SiteMapping,
                });

                if (++i % 2000 == 0 || i == sources.Count)
                {
                    try
                    {
                        RMRestoreSiteMappingDao.CreateByBulkCopyAsync(siteMappings).GetAwaiter().GetResult();
                        mSucceedCount += siteMappings.Count;
                    }
                    catch (Exception e)
                    {
                        mLog.Error($"Fail batch save site mapping, count:{siteMappings.Count}, ex:{e}");
                        foreach(var item in siteMappings)
                        {
                            mLog.Error($"Fail save item: {item.SourceSiteUrl} : {item.TargetSiteUrl}");
                        }
                        mFailedCount += siteMappings.Count;
                    }
                    finally
                    {
                        siteMappings.Clear();
                    }
                    maxIntId = RMRestoreSiteMappingDao.GetLastMappingIntId();
                }
            }
            JobMonitorService.UpdateJobProgress(mJobId, 90);
        }

        private void CheckJobIsStoping()
        {
            if (CheckJobStatusUtility.isStopping)
            {
                throw new JobStopException();
            }
        }


        private void UpdateJobStatus()
        {

            if (CheckJobStatusUtility.isStopping)
            {
                JobMonitorService.UpdateJobStatus(mJobId, JobStatus.Stopped);
            }
            else if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                JobMonitorService.UpdateJobStatus(mJobId, JobStatus.Failed, errorMessage);
                mLog.Info("Import term job failed.");
            }else if (mFailedCount == 0)
            {
                JobMonitorService.UpdateJobStatus(mJobId, JobStatus.Finished);
                mLog.Info("Import term job finished.");
            }
            else if (mSucceedCount == 0)
            {
                JobMonitorService.UpdateJobStatus(mJobId, JobStatus.Failed, "RM_RS_SiteMappings_ErrorMessage");
                mLog.Info("Import term job failed.");
            }
            else if (mSucceedCount > 0 && mFailedCount > 0)
            {
                JobMonitorService.UpdateJobStatus(mJobId, JobStatus.FinishWithException, "RM_RS_SiteMappings_PartDataSaveFail");
                mLog.Info("Import term job finished with exception.");
            }
        }




    }
}
