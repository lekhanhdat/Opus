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
using AvePoint.GCommon.Contract.AveLicense;
using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Threads;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.I18N.Core;
using AvePoint.Wrapper.Common;
using Google.Api.Gax.ResourceNames;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace AvePoint.RA.Common.Util
{
    public class JobReportUtility
    {
        private static RALogger logger = RALogger.GetInstance(typeof(JobReportUtility));
        public const string CommonFolder = "common";
        public const string TemplateFolder = "Template";
        public const string ImportCSVFile = "ImportCSVFile";
        public const string FullTextIndexJobInfoFile = "FullTextIndexPayloadFile";
        public const string ImportSCMappingFile = "ImportSCMappingFile";
        public const string ImportSCMappingFolder = "ImportSCMapping";
        public const string ImportSCWhitelistFile = "ImportSCWhitelistFile";
        public const string ImportSCWhitelistFolder = "ImportSCWhitelist";
        public const string ImportSCBlacklistFile = "ImportSCBlacklistFile";
        public const string ImportSCBlacklistFolder = "ImportSCBlacklist";
        public const string ImportDiscoveryExcludeListFile = "ImportDiscoveryExcludeListFile";
        public const string ImportDiscoveryExcludeListFolder = "ImportDiscoveryExcludeList";
        public const string ExportVEOConfig = "Export VEO Config";
        public const string VEOConfigZip = "VEO Configuration Files.zip";
        public const string MachineLearningFolder = "Machine Learning";
        public const string DiscoveryDuplicationReportZip = "Discovery O365 Duplication Report.zip";

        public const string ExportNAAConfig = "Export NAA Config";
        public const string NAAVEOConfigZip = "NAA Configuration Files.zip";
        public const string ExportNARAConfig = "Export NARA Config";
        public const string NARAVEOConfigZip = "NARA Configuration Files.zip";
        private const string I18NSTR = "RM_";
        public static string REPORT_FOLDER
        {
            get
            {
                return RMGlobalConfiguration.StorageConfig[Contract.Configurations.RMStorageSettingKey.REPORT_TEMP_FOLDER];
            }
        }

        /// <summary>
        /// 获取job report的路径
        /// </summary>
        /// <remarks>
        /// 当expandedName为空时取得Job report 文件夹路径
        /// </remarks>
        /// <param name="jobDto">job</param>
        /// <param name="expandedName">文件扩展名,该值可空、</param>
        public static string GetJobReportPath(BaseJobDto jobDto, string expandedName = "")
        {
            //string tenantIdentity = CallContext.LogicalGetData("TenantIdentity") as string; //this value set in LoggerInitializer.Initialize();
            string tenantIdentity = TenantLocalValue.LogonGroupId; //this value set in LoggerInitializer.Initialize();
            if (string.IsNullOrEmpty(tenantIdentity))
            {
                tenantIdentity = CommonFolder;
            }
            return InnerGetJobReportPath(tenantIdentity, jobDto, false, expandedName);
        }

        public static bool CheckInSOReportTypes(int jobType)
        {
            if (JobTypeConstants.SOSPReportTypes.Contains(jobType) || JobTypeConstants.SOOneDriveReportTypes.Contains(jobType) || JobTypeConstants.SOTeamsReportTypes.Contains(jobType))
            {
                return true;
            }
            return false;
        }

        public static string GetJobReportTempPath(BaseJobDto jobDto, string expandedName = "")
        {

            return InnerGetJobReportPath(GetTenantIdentity(), jobDto, true, expandedName);
        }


        public static string GetArchiverJobReportPath(BaseJobDto jobDto, string expandedName = "")
        {
            var rootFolder = AvePoint.GCommon.Utility.SecurityUtils.SafeCombinePath(REPORT_FOLDER, "Temp");
            string jobReportPath = AvePoint.GCommon.Utility.SecurityUtils.SafeCombinePath (rootFolder, GetTenantIdentity());
            jobReportPath = AvePoint.GCommon.Utility.SecurityUtils.SafeCombinePath(jobReportPath, AssembleReportPathAfterHalf(jobDto.PlanId, jobDto.Id, jobDto.Category.Value, expandedName));
            return jobReportPath;
        }

        public static string GetTenantIdentity()
        {
            //TODO Azure login user;
            //return TenantLocalValue.LogonUserEmail;
            return TenantLocalValue.LogonGroupId;
        }

        private static string InnerGetJobReportPath(string tenantIdentity, BaseJobDto jobDto, bool isTempSub, string expandedName = "")
        {
            
            var rootFolder = isTempSub ? AvePoint.GCommon.Utility.SecurityUtils.SafeCombinePath(REPORT_FOLDER, "Temp") : REPORT_FOLDER;
            string jobReportPath = AvePoint.GCommon.Utility.SecurityUtils.SafeCombinePath(rootFolder, tenantIdentity);
            if (!string.IsNullOrEmpty(jobDto.SiteCollectionUrl))
            {
                jobReportPath = AvePoint.GCommon.Utility.SecurityUtils.SafeCombinePath(jobReportPath,AssembleReportPathAfterHalfForStatisticsSoSize(jobDto.Id, jobDto.JobType, jobDto.SiteCollectionUrl, expandedName));
            }
            else
            {
                jobReportPath = AvePoint.GCommon.Utility.SecurityUtils.SafeCombinePath(jobReportPath, AssembleReportPathAfterHalf(jobDto.Id, jobDto.JobType, expandedName));
            }
            return jobReportPath;
        }

        public static string GetRestoreReportJobScDetailPath(string ScUrl)
        {
            string webAppName;
            string siteName;
            ParseSitePath(ScUrl, out webAppName, out siteName);
            return SecurityUtils.SafeCombinePath(REPORT_FOLDER, GetTenantIdentity(),
                JobMonitorConstants.RESTORE_REPORT_SC_FOLDER, webAppName, siteName+".rpt");
        }
        public static string GetRestoreReportJobGDDetailPath(string driveId)
        {
            return SecurityUtils.SafeCombinePath(REPORT_FOLDER, GetTenantIdentity(),
                JobMonitorConstants.RESTORE_REPORT_GD_FOLDER, driveId + ".rpt");
        }

        public static string GetRestoreReportJobScDetailUri(string ScUrl)
        {
            string webAppName;
            string siteName;
            ParseSitePath(ScUrl, out webAppName, out siteName);
            return SecurityUtils.SafeCombinePath(GetTenantIdentity(),
                JobMonitorConstants.RESTORE_REPORT_SC_FOLDER, webAppName, siteName+".rpt");
        }
        public static string GetRestoreReportJobGDDetailUri(string driveName)
        {
            return SecurityUtils.SafeCombinePath(GetTenantIdentity(),
                JobMonitorConstants.RESTORE_REPORT_GD_FOLDER, driveName + ".rpt");
        }

        protected static void ParseSitePath(String siteURL, out String webAppName, out String siteName)
        {
            int index = -1;
            StringBuilder tmp = new StringBuilder();
            index = siteURL.IndexOf(":", StringComparison.OrdinalIgnoreCase);
            tmp.Append(siteURL.Substring(0, index)).Append("#");
            string temp = siteURL.Substring(index + 3);
            index = -1;
            index = temp.IndexOf(":", StringComparison.OrdinalIgnoreCase);
            if (index == -1)
            {
                tmp.Append(80).Append("#");
                index = temp.IndexOf("/", StringComparison.OrdinalIgnoreCase);
                if (index != -1)
                {
                    tmp.Append(temp.Substring(0, index));
                    temp = temp.Substring(index + 1);
                }
                else
                {
                    tmp.Append(temp);
                    temp = "";
                }
            }
            else
            {
                String machineName = temp.Substring(0, index);
                temp = temp.Substring(index + 1);
                index = -1;
                index = temp.IndexOf("/", StringComparison.OrdinalIgnoreCase);
                if (index != -1)
                {
                    tmp.Append(temp.Substring(0, index));
                    temp = temp.Substring(index + 1);
                }
                else
                {
                    tmp.Append(temp);
                    temp = "";
                }
                tmp.Append("#").Append(machineName);
            }
            webAppName = tmp.ToString();
            tmp.Remove(0, tmp.Length);
            tmp.Append("#");
            if (temp.Length > 0)
            {
                temp = temp.Replace(';', '#');
                tmp.Append(temp.Replace('/', '#'));
            }
            siteName = tmp.ToString();
        }


        /// <summary>
        /// 获取job report的路径
        /// </summary>
        /// <remarks>
        /// 当expandedName为空时取得Job report 文件夹路径
        /// </remarks>
        /// <param name="jobDto">job</param>
        /// <param name="expandedName">文件扩展名,该值可空、</param>
        public static string GetDownloadReportDetailTempleFolder(BaseJobDto jobDto, string expandedName = "")
        {
            return AvePoint.GCommon.Utility.SecurityUtils.SafeCombinePath(
                AssembleReportPathForeBody() + JobMonitorConstants.REPORT_TEMPLE_FOLDER,
                jobDto.Id, jobDto.Id + expandedName);
        }
        public static string GetDownloadReportDetailTempleFolder(BaseJobDto jobDto)
        {
            return AvePoint.GCommon.Utility.SecurityUtils.SafeCombinePath(
                AssembleReportPathForeBody() + JobMonitorConstants.REPORT_TEMPLE_FOLDER, jobDto.Id);
        }

        public static string GetDownloadReportDetailTempleFolder(BaseJobDto jobDto,string fileName, string expandedName = "")
        {
            return AvePoint.GCommon.Utility.SecurityUtils.SafeCombinePath(
                AssembleReportPathForeBody() + JobMonitorConstants.REPORT_TEMPLE_FOLDER,
                jobDto.Id, fileName + expandedName);
        }

        public static string GetDownloadRuleUsageReportTempleFolder(string FolderName)
        {
            StringBuilder stringBuild = new StringBuilder();
            stringBuild.Append(AssembleReportPathForeBody());
            stringBuild.Append("Rule Usage Report").Append(Path.DirectorySeparatorChar);
            stringBuild.Append(FolderName);
            return stringBuild.ToString();
        }

        public static string GetDownloadTermInfoReportTempleFolder(string FolderName)
        {
            StringBuilder stringBuild = new StringBuilder();
            stringBuild.Append(AssembleReportPathForeBody());
            stringBuild.Append(I18N.Core.I18NEntity.GetString("RM_JS_TM_ExportTerm")).Append(Path.DirectorySeparatorChar);
            stringBuild.Append(FolderName);
            return stringBuild.ToString();
        }
        
        public static string GetDownloadExportRowDataInfoReportTempleFolder(string folderName)
        {
            StringBuilder stringBuild = new StringBuilder();
            stringBuild.Append(AssembleReportPathForeBody());
            stringBuild.Append(I18N.Core.I18NEntity.GetString("ExportRowDataJob")).Append(Path.DirectorySeparatorChar);
            stringBuild.Append(folderName);
            return stringBuild.ToString();
        }

        public static string GetDownloadDiscoveryExportDuplicationReportTempleFolder(string folderName)
        {
            StringBuilder stringBuild = new StringBuilder();
            stringBuild.Append(AssembleReportPathForeBody());
            stringBuild.Append("Discovery O365 Duplication Report").Append(Path.DirectorySeparatorChar);
            stringBuild.Append(folderName);
            return stringBuild.ToString();
        }

        public static string GetDownloadSharePointReportSiteExportTempleFolder(string folderName)
        {
            StringBuilder stringBuild = new StringBuilder();
            stringBuild.Append(AssembleReportPathForeBody());
            stringBuild.Append("SharePoint Report Export Site").Append(Path.DirectorySeparatorChar);
            stringBuild.Append(folderName);
            return stringBuild.ToString();
        }

        public static string GetDownloadDiscoveryExportReportTempleFolder(string FolderName)
        {
            StringBuilder stringBuild = new StringBuilder();
            stringBuild.Append(AssembleReportPathForeBody());
            stringBuild.Append(I18N.Core.I18NEntity.GetString("Export Discovery O365 Data")).Append(Path.DirectorySeparatorChar);
            stringBuild.Append(FolderName);
            return stringBuild.ToString();
        }

        public static string GetDownloadBarcodeInfoReportTempleFolder(string FolderName)
        {
            StringBuilder stringBuild = new StringBuilder();
            stringBuild.Append(AssembleReportPathForeBody());
            stringBuild.Append(I18N.Core.I18NEntity.GetString("Export Barcode")).Append(Path.DirectorySeparatorChar);
            stringBuild.Append(FolderName);
            return stringBuild.ToString();
        }
        public static string GetDownloadHoldRecordReportTempleFolder(string FolderName)
        {
            StringBuilder stringBuild = new StringBuilder();
            stringBuild.Append(AssembleReportPathForeBody());
            stringBuild.Append(I18N.Core.I18NEntity.GetString("Export Holds Records")).Append(Path.DirectorySeparatorChar);
            stringBuild.Append(FolderName);
            return stringBuild.ToString();
        }

        public static string GetDownloadRecordExportReportTempleFolder(string FolderName)
        {
            return AvePoint.GCommon.Utility.SecurityUtils.SafeCombinePath(
                AssembleReportPathForeBody() + I18N.Core.I18NEntity.GetString("Export Report"), FolderName);
        }
        public static string GetDownloadPhysicalBulkImportTempleFolder(string FolderName)
        {
            StringBuilder stringBuild = new StringBuilder();
            stringBuild.Append(AssembleReportPathForeBody());
            stringBuild.Append("Physical Bulk Import").Append(Path.DirectorySeparatorChar);
            stringBuild.Append(FolderName);
            return stringBuild.ToString();
        }
        public static string GetDownloadHoldRecordsBulkImportTempleFolder(string FolderName)
        {
            StringBuilder stringBuild = new StringBuilder();
            stringBuild.Append(AssembleReportPathForeBody());
            stringBuild.Append("Hold Records Bulk Import").Append(Path.DirectorySeparatorChar);
            stringBuild.Append(FolderName);
            return stringBuild.ToString();
        }

        public static string GetDownloadPhysicalBulkImportZipTempleFolder(string FolderName)
        {
            StringBuilder stringBuild = new StringBuilder();
            stringBuild.Append(AssembleReportPathForeBody());
            stringBuild.Append("Physical Bulk Zip Import").Append(Path.DirectorySeparatorChar);
            stringBuild.Append(FolderName);
            return stringBuild.ToString();
        }

        //public static string GetDownloadLoanBoxTempleFolder(string FolderName)
        //{
        //    StringBuilder stringBuild = new StringBuilder();
        //    stringBuild.Append(AssembleReportPathForeBody());
        //    stringBuild.Append(I18N.Core.I18NEntity.GetString("Loan Box")).Append(Path.DirectorySeparatorChar);
        //    stringBuild.Append(FolderName);
        //    return stringBuild.ToString();
        //}

        public static string GetDownloadSetPermissionInfoReportTempleFolder(string FolderName)
        {
            StringBuilder stringBuild = new StringBuilder();
            stringBuild.Append(AssembleReportPathForeBody());
            stringBuild.Append(I18N.Core.I18NEntity.GetString("Physical Set Permission")).Append(Path.DirectorySeparatorChar);
            stringBuild.Append(FolderName);
            return stringBuild.ToString();
        }

        public static string GetDownloadAuditReportTempleFolder(string FolderName)
        {
            StringBuilder stringBuild = new StringBuilder();
            stringBuild.Append(AssembleReportPathForeBody());
            stringBuild.Append("Administrator Auditor Report").Append(Path.DirectorySeparatorChar);
            stringBuild.Append(FolderName);
            return stringBuild.ToString();
        }

        /// <summary>
        ///  Job Monitor中勾选job,下载对应job Detail的临时文件夹
        /// </summary>
        /// <returns></returns>
        public static string GetDownloadJobMonitorDetailTempleFolder(string pathExtension)
        {
            StringBuilder stringBuild = new StringBuilder();
            stringBuild.Append(AssembleReportPathForeBody());
            stringBuild.Append(JobMonitorConstants.REPORT_TEMPLE_FOLDER);
            if (!string.IsNullOrEmpty(pathExtension))
            {
                stringBuild.Append(Path.DirectorySeparatorChar + pathExtension);
            }
            return stringBuild.ToString();
        }

        public static string GetManualApprovalReportTempleFolder(BaseJobDto jobDto)
        {
            StringBuilder stringBuild = new StringBuilder();
            stringBuild.Append(AssembleReportPathForeBody());
            stringBuild.Append("MAReport").Append(Path.DirectorySeparatorChar);
            stringBuild.Append(jobDto.Id);
            return stringBuild.ToString();
        }

        public static string GetDownloadArchiverSiteInfoReportTempleFolder(string FolderName)
        {
            StringBuilder stringBuild = new StringBuilder();
            stringBuild.Append(AssembleReportPathForeBody());
            stringBuild.Append(I18N.Core.I18NEntity.GetString("RM_AR_Report_ExportArchiverSite")).Append(Path.DirectorySeparatorChar);
            stringBuild.Append(FolderName);
            return stringBuild.ToString();
        }

        public static string GetDownloadArchiveDepulicationSiteInfoReportTempleFolder(string FolderName)
        {
            StringBuilder stringBuild = new StringBuilder();
            stringBuild.Append(AssembleReportPathForeBody());
            stringBuild.Append(I18N.Core.I18NEntity.GetString("RM_AR_Report_ExportArchiverDepulicationSite")).Append(Path.DirectorySeparatorChar);
            stringBuild.Append(FolderName);
            return stringBuild.ToString();
        }

        public static string GetDownloadsSiteCollectionMappingTempleFolder(string FolderName)
        {
            StringBuilder stringBuild = new StringBuilder();
            stringBuild.Append(AssembleReportPathForeBody());
            stringBuild.Append(ImportSCMappingFolder).Append(Path.DirectorySeparatorChar);
            stringBuild.Append(FolderName);
            return stringBuild.ToString();
        }

        public static string GetDownloadsSiteCollectionWhitelistTempleFolder(string FolderName)
        {
            StringBuilder stringBuild = new StringBuilder();
            stringBuild.Append(AssembleReportPathForeBody());
            stringBuild.Append(ImportSCWhitelistFolder).Append(Path.DirectorySeparatorChar);
            stringBuild.Append(FolderName);
            return stringBuild.ToString();
        }


        public static string GetDownloadsDiscoveryExcludeSCTempleFolder(string folderName)
        {
            return GetDownloadsTemplateFolder(ImportDiscoveryExcludeListFolder, folderName);
        }

        private static string GetDownloadsTemplateFolder(string rootFolder, string folderName)
        {
            StringBuilder stringBuild = new StringBuilder();
            stringBuild.Append(AssembleReportPathForeBody());
            stringBuild.Append(rootFolder).Append(Path.DirectorySeparatorChar);
            stringBuild.Append(folderName);
            return stringBuild.ToString();
        }

        public static string GetDownloadManualApprovalReviewReportTempleFolder(string FolderName)
        {
            StringBuilder stringBuild = new StringBuilder();
            stringBuild.Append(AssembleReportPathForeBody());
            stringBuild.Append(I18N.Core.I18NEntity.GetString("^ManualApprovalReviewReport")).Append(Path.DirectorySeparatorChar);
            stringBuild.Append(FolderName);
            return stringBuild.ToString();
        }

        public static string GetDownloadLoanPickListReportTempleFolder(string FolderName)
        {
            StringBuilder stringBuild = new StringBuilder();
            stringBuild.Append(AssembleReportPathForeBody());
            stringBuild.Append(I18N.Core.I18NEntity.GetString("RM_JS_Phy_ReturnHistoryExport")).Append(Path.DirectorySeparatorChar);
            stringBuild.Append(FolderName);
            return stringBuild.ToString();
        }
        public static string GetDownloadDestructionPickListReportTempleFolder(string FolderName)
        {
            StringBuilder stringBuild = new StringBuilder();
            stringBuild.Append(AssembleReportPathForeBody());
            stringBuild.Append(I18N.Core.I18NEntity.GetString("RM_JS_Phy_DestructionPickExport")).Append(Path.DirectorySeparatorChar);
            stringBuild.Append(FolderName);
            return stringBuild.ToString();
        }
        public static string GetMLTempleFolder(string folderName)
        {
            StringBuilder stringBuild = new StringBuilder();
            stringBuild.Append(AssembleReportPathForeBody());
            stringBuild.Append(MachineLearningFolder).Append(Path.DirectorySeparatorChar);
            stringBuild.Append(folderName);
            return stringBuild.ToString();
        }

        public static void CheckAndCreateDirectory(string path)
        {

            FileInfo reportFile = new FileInfo(path);
            if (!reportFile.Directory.Exists)
            {
                reportFile.Directory.Create();
            }

        }

        private static string AssembleReportPathForeBody()
        {
            return Path.Combine(REPORT_FOLDER, JobMonitorConstants.JOB_REPORT_FOLDER) + Path.DirectorySeparatorChar;
        }
        private static string AssembleReportPathAfterHalfForStatisticsSoSize(string jobId, int jobType, string siteUrl, string expandedName)
        {
            StringBuilder stringBuild = new StringBuilder();
            string moduleName = "SoJobSizeStatisticsReport";
            stringBuild.Append(moduleName);
            stringBuild.Append(Path.DirectorySeparatorChar);
            stringBuild.Append(jobId);
            stringBuild.Append(expandedName);

            return stringBuild.ToString();
        }
        private static string AssembleReportPathAfterHalf(string jobId, int jobType, string expandedName)
        {
            StringBuilder stringBuild = new StringBuilder();
            string moduleName = string.Empty;
            if (jobType == (int)JobType.TermSynchronization || jobType == (int)JobType.PhysicalTermSynchronization)
            {
                moduleName = "Term Synchronisation";
            }
            else if (jobType == (int)JobType.ItemsFilesDueDisposal || jobType == (int)JobType.EXOItemsFilesDueDisposalReport
                || jobType == (int)JobType.PhysicalItemsFilesDueDisposalReport || jobType == (int)JobType.FSItemsFilesDueDisposal
                || jobType == (int)JobType.OneDriveItemsFilesDueDisposalReport || jobType == (int)JobType.SPOnPremItemsFilesDueDisposal
                || jobType == (int)JobType.DisposalReport || jobType == (int)JobType.BoxItemsFilesDueDisposalReport
                || jobType == (int)JobType.GoogleItemsFilesDueDisposalReport
                || jobType == (int)JobType.TeamsItemsFilesDueDisposalReport)
            {
                moduleName = "Content Due for Disposal Report";
            }
            else if (jobType == (int)JobType.BCSTermUsageReport || jobType == (int)JobType.EXOTermUsageReport
                || jobType == (int)JobType.PhysicalTermUsageReport || jobType == (int)JobType.FSBCSTermUsageReport
                || jobType == (int)JobType.OneDriveTermUsageReport || jobType == (int)JobType.SPOnPremBCSTermUsageReport
                || jobType == (int)JobType.TermUsageReport || jobType == (int)JobType.BoxBCSTermUsageReport
                || jobType == (int)JobType.GoogleBCSTermUsageReport || jobType == (int)JobType.TeamsBCSTermUsageReport)
            {
                moduleName = "Term Usage Report";
            }
            else if (jobType == (int)JobType.SharePointGlobalSetting || jobType == (int)JobType.SharePointScheduleSetting || jobType == (int)JobType.ApplySharePointSettings)
            {
                moduleName = "SharePoint Settings";
            }
            else if (jobType == (int)JobType.TeamsScheduleSetting || jobType == (int)JobType.ApplyTeamsSettings)
            {
                moduleName = "Teams Settings";
            }
            else if (jobType == (int)JobType.PhysicalFolderSynchronization)
            {
                moduleName = "Physical Folder Synchronisation";
            }
            else if (jobType == (int)JobType.UpdateLocation)
            {
                moduleName = "Update Location";
            }
            else if (jobType == (int)JobType.ImportPhysicalRecords || jobType == (int)JobType.ImportRecordsRelated || jobType == (int)JobType.TrimRecordsDeletion)
            {
                moduleName = "Import Physical Records";
            }
            else if (jobType == (int)JobType.CreateAndDestroyedFileReport
                || jobType == (int)JobType.EXOCreateAndDestroyedFileReport
                || jobType == (int)JobType.PhysicalCreateAndDestroyedFileReport
                || jobType == (int)JobType.FSCreateAndDestroyedFileReport
                || jobType == (int)JobType.OneDriveCreateAndDestroyedFileReport
                || jobType == (int)JobType.SPOnPremCreateAndDestroyedFileReport
                || jobType == (int)JobType.CreateAndDestroyedReport
                || jobType == (int)JobType.BoxCreateAndDestroyedFileReport
                || jobType == (int)JobType.GoogleCreateAndDestroyedFileReport
                || jobType == (int)JobType.TeamsCreateAndDestroyedFileReport)
            {
                moduleName = "Content Due for Time Frame Report";
            }
            else if (jobType == (int)JobType.RestoreReport || jobType == (int)JobType.OneDriverRestoreReport || jobType == (int)JobType.TeamsRestoreReport)
            {
                moduleName = "Restore Report";
            }
            else if (jobType == (int)JobType.GoogleRestoreReport)
            {
                moduleName = "Google Restore Report";
            }
            else if (jobType == (int)JobType.AvailableSpaceReport)
            {
                moduleName = "Available Space Report";
            }
            else if (jobType == (int)JobType.ImportTermStructure || jobType == (int)JobType.ImportGoogleTermStructure)
            {
                moduleName = "Term Import";
            }
            else if (jobType == (int)JobType.ExportTermStructure)
            {
                moduleName = "Term Export";
            }
            else if (jobType == (int)JobType.PhysicalTemplateImport)
            {
                moduleName = "Physical Template Import";
            }
            else if (jobType == (int)JobType.UniqueIDSettingIncrementalSchedule || jobType == (int)JobType.UniqueIDSettingFullSchedule
                || jobType == (int)JobType.TeamsUniqueIDSettingIncrementalSchedule || jobType == (int)JobType.TeamsUniqueIDSettingFullSchedule)
            {
                moduleName = "UniqueID Setting Incremental Schedule";
            }
            else if (jobType == (int)JobType.EnforceRetention || jobType == (int)JobType.OldEnforceRetention)
            {
                moduleName = "Enforce Retention";
            }
            else if (jobType == (int)JobType.CollectionDataFull)
            {
                moduleName = "Collection Data Full Schedule";
            }
            else if (jobType == (int)JobType.CollectionDataIncremental)
            {
                moduleName = "Collection Data Incremental Schedule";
            }
            else if (jobType == (int)JobType.ManualApprovalTimer || jobType == (int)JobType.ManualApproval)
            {
                moduleName = "Manual Approval Timer";
            }
            else if (jobType == (int)JobType.ArchiverFullTextIndex)
            {
                moduleName = "Archiver Full Text Index";
            }
            else if (jobType == (int)JobType.DeleteRestoredData)
            {
                moduleName = "Delete Restored Data";
            }
            else if (jobType == (int)JobType.DiscoveryJobV2 || jobType == (int)JobType.DiscoveryJobV3 || jobType == (int)JobType.DiscoveryJobV4 || jobType == (int)JobType.DiscoveryJobV5 || jobType == (int)JobType.DiscoveryAOSPJob)
            {
                moduleName = "Discovery And Analysis Job";
            }
            else if (jobType == (int)JobType.DiscoveryGoogleJobV1)
            {
                moduleName = "Discovery And Analysis Google Job";
            }
            else if (jobType == (int)JobType.DiscoveryProfileJob)
            {
                moduleName = "Analysis Profile Job";
            }
            else if (jobType == (int)JobType.DiscoveryExportO365Profile)
            {
                moduleName = "Discovery Profile Profile Job";
            }
            else if (jobType == (int)JobType.SharePointOnlineDeletionSyncUpgrade)
            {
                moduleName = "SharePoint Online Deletion Sync Upgrade";
            }
            else if (jobType == (int)JobType.SendEmailJob)
            {
                moduleName = "Send email job";
            }
            else if (jobType == (int)JobType.ManualFileSystemUpgrade)
            {
                moduleName = "Manual Approval File System Upgrade";
            }
            else if (jobType == (int)JobType.DiscoveryJob)
            {
                moduleName = "Discovery";
            }
            else if (jobType == (int)JobType.DiscoveryOptimizationCalculate)
            {
                moduleName = "Discovery Optimization Calculate";
            }
            else if (jobType == (int)JobType.DiscoveryAOSPOptimizationCalculate)
            {
                moduleName = "Discovery AOSP Optimization Calculate";
            }
            else if (jobType == (int)JobType.DiscoveryReCalculate)
            {
                moduleName = "Discovery Re Calculate";
            }
            else if (jobType == (int)JobType.CosmosDBDirtyDataDeleteUpgrade)
            {
                moduleName = "Cosmos DB Dirty Data Delete Upgrade";
            }
            else if (jobType == (int)JobType.DataSynchronisation || jobType == (int)JobType.SPDataSynchronisationSchedule
               || jobType == (int)JobType.OneDriveDataSynchronisation || jobType == (int)JobType.OneDriveDataSynchronisationSchedule)
            {
                moduleName = "Data Synchronisation";
            }
            else if (jobType == (int)JobType.AzureFileShareDataSynchronisation || jobType == (int)JobType.AzureFileShareDataSynchronisationSchedule)
            {
                moduleName = "Azure File Share Data Synchronisation";
            }
            else if (jobType == (int)JobType.RecordsExplorerMove)
            {
                moduleName = "Explorer Move";
            }
            else if (jobType == (int)JobType.EXOApplySetting || jobType == (int)JobType.EXOApplySettingSchedule)
            {
                moduleName = "EXO ApplySetting";
            }
            else if (jobType == (int)JobType.EXODataSynchronisation || jobType == (int)JobType.EXODataSynchronisationSchedule)
            {
                moduleName = "EXO DataSync";
            }
            else if (jobType == (int)JobType.EXORecordsDisposal)
            {
                moduleName = "EXO Enforce Rule Action";
            }
            else if (jobType == (int)JobType.RecordsDisposal)
            {
                moduleName = "SPO Enforce Rule Action";
            }
            else if (jobType == (int)JobType.OneDriveRecordsDisposal)
            {
                moduleName = "OneDrive Enforce Rule Action";
            }
            else if (jobType == (int)JobType.PhysicalDisposal || jobType == (int)JobType.PhysicalRecordsDisposal)
            {
                moduleName = "Physical Disposal";
            }
            else if (jobType == (int)JobType.PhysicalExplorerTimer)
            {
                moduleName = "Physical Explorer Timer";
            }
            else if (jobType == (int)JobType.ConnectorTimer)
            {
                moduleName = "Connector Timer";
            }
            else if (jobType == (int)JobType.ImportSPSetting)
            {
                moduleName = "Import SharePoint Setting";
            }
            else if (jobType == (int)JobType.PhysicalExportBarcode)
            {
                moduleName = "Physical Export Barcode";
            }
            else if (jobType == (int)JobType.ActionOnly)
            {
                moduleName = "ActionOnly";
            }
            else if (jobType == (int)JobType.PhysicalSetPermission)
            {
                moduleName = "Physical Set Permission";
            }
            else if (jobType == (int)JobType.FSFolderChangeTerm)
            {
                moduleName = "FS Folder Reclassify";
            }
            else if (jobType == (int)JobType.FSFolderManageHold)
            {
                moduleName = "FS Folder Hold";
            }
            else if (jobType == (int)JobType.GlobalSearchAction)
            {
                moduleName = "GlobalSearchAction";
            }
            else if (jobType == (int)JobType.ExportSearchResult)
            {
                moduleName = "ExportSearchResult";
            }
            else if (jobType == (int)JobType.ExplorerOfflineSearch)
            {
                moduleName = "OfflineSearchResult";
            }
            else if (jobType == (int)JobType.SPOnPremDataSync || jobType == (int)JobType.SPOnPremDataSyncSchedule)
            {
                moduleName = "SPOnPremDataSync";
            }
            else if (jobType == (int)JobType.SPOnPremApplySetting)
            {
                moduleName = "SPOnPremApplySetting";
            }
            else if (jobType == (int)JobType.SPOnPremUniqueIDSettingFullSchedule || jobType == (int)JobType.SPOnPremUniqueIDSettingIncrementalSchedule)
            {
                moduleName = "SPOnPremUniqueIDSettingFullSchedule";
            }
            else if (jobType == (int)JobType.PhysicalLoanBox || jobType == (int)JobType.PhysicalReturnBox)
            {
                moduleName = "PhysicalLoanBox";
            }
            else if (jobType == (int)JobType.FSDataSynchronization || jobType == (int)JobType.FSDataSynchronizationSchedule)
            {
                moduleName = "FSDataSynchronization";
            }
            else if (jobType == (int)JobType.FSDisposal || jobType == (int)JobType.FSDisposalSchedule)
            {
                moduleName = "FSDisposal";
            }
            else if (jobType == (int)JobType.FSDisposalByClassCode) 
            { 
                moduleName = "FSDisposalByClassCode";
            }
            else if (jobType == (int)JobType.ImportFSSetting)
            {
                moduleName = "Import FS Setting";
            }
            else if (jobType == (int)JobType.SPOActionAuditReport || jobType == (int)JobType.OneDriveActionAuditReport || jobType == (int)JobType.TeamsActionAuditReport)
            {
                moduleName = "Client Audit Report";
            }
            else if (jobType == (int)JobType.PhysicalLoanPick)
            {
                moduleName = "Physical Loan Pick";
            }
            else if (jobType == (int)JobType.PhysicalDestructionPick)
            {
                moduleName = "Physical Destruction Pick";
            }
            else if (jobType == (int)JobType.GenerateRestoreReport)
            {
                moduleName = "Has Generated Restore Report";
            }
            else if (jobType == (int)JobType.ArchiverRestore || jobType == (int)JobType.StubOopRestore || jobType == (int)JobType.AOSPRestore || jobType == (int)JobType.ArchiverToSpoRestore || jobType == (int)JobType.StubArchiverRestore || jobType == (int)JobType.M365InPlaceArchiverRestore)
            {
                if (jobId.StartsWith("ORS"))
                {
                    moduleName = JobType.ArchiverOutPlaceRestore.ToString();
                }
                else
                {
                    moduleName = "ArchiverRestore";
                }
            }
            else if (jobType == (int)JobType.BoxDataSynchronisation)
            {
                moduleName = "Data Synchronisation For Box";
            }
            else if (jobType == (int)JobType.BoxDataSynchronisationSchedule)
            {
                moduleName = "Data SynchronisationSchedule For Box";
            }
            else if (jobType == (int)JobType.BoxRecordsDisposal)
            {
                moduleName = "Run Enforce Rule For Box";
            }
            else if (jobType == (int)JobType.ExportFSSetting)
            {
                moduleName = "Export File System setting";
            }
            else if (jobType == (int)JobType.DownloadRCCReport)
            {
                moduleName = "Download RCC report";
            }
            else if (jobType == (int)JobType.ExportSPSetting)
            {
                moduleName = "Export Share point setting";
            }
            else if (jobType == (int)JobType.ExportSPSOSetting)
            {
                moduleName = "Export Storage Optimize SharePoint Setting";
            }
            else if (jobType == (int)JobType.ExportTeamsSOSetting)
            {
                moduleName = "Export Storage Optimize Teams Setting";
            }
            else if (jobType == (int)JobType.DeleteOrphanDatas)
            {
                moduleName = "DeleteOrphanDatas";
            }
            else if (jobType == (int)JobType.ConvertStub)
            {
                moduleName = "Convert Stub";
            }
            else if (jobType == (int)JobType.DeclaredRecordsMigration)
            {
                moduleName = "Declared Records Migration";
            }
            else if (jobType == (int)JobType.StubDisposal)
            {
                moduleName = "Stub Disposal";
            }
            else if (jobType == (int)JobType.DeleteArchivedSiteCollection)
            {
                moduleName = "Delete Archived Site Collection";
            }
            else if (jobType == (int)JobType.TeamsNodeSettingUpgrade)
            {
                moduleName = "Teams Node Setting Upgrade";
            }
            else if (jobType == (int)JobType.ImportSCWhitelist)
            {
                moduleName = "Import Site Collection Whitelist";
            }
            else if (jobType == (int)JobType.ImportSCBlacklist)
            {
                moduleName = "Import Site Collection Blacklist";
            }
            else if (jobType == (int)JobType.TeamsDataSynchronisation || jobType == (int)JobType.TeamsDataSynchronisationSchedule)
            {
                moduleName = "Teams Data Synchronisation";
            }
            else if (jobType == (int)JobType.TeamsRecordsDisposal)
            {
                moduleName = "Teams Enforce Rule Action";
            }
            else if (jobType == (int)JobType.TeamsArchiverRestore)
            {
                moduleName = "Teams Archiver Restore";
            }
            else if (jobType == (int)JobType.TeamsOutPlaceRestore)
            {
                moduleName = "Teams Out Place Restore";
            }
            else if (jobType == (int)JobType.MailBoxArchiverRestore)
            {
                moduleName = "Mailbox Archiver Restore";
            }
            else if (jobType == (int)JobType.ArchiverRetentionSimulate || jobType == (int)JobType.FSRetainSimulate)
            {
                moduleName = "ArchiverRetentionSimulate";
            }
            else if (jobType == (int)JobType.GoogleArchiverRestore)
            {
                moduleName = "Google Archiver Restore";
            }
            else if (jobType == (int)JobType.ApplyClassCode)
            {
                moduleName = "ApplyClassCode";
            }
            else if (jobType == (int)JobType.DiscoveryExportExcludeSCList)
            {
                moduleName = "Discovery Export Exclude Site Collection List";
            } 
            else if(jobType == (int)JobType.DiscoveryImportExcludeSCList)
            {
                moduleName = "Discovery Import Exclude Site Collection List";
            }
            else if (jobType == (int)JobType.ImportHoldRecords)
            {
                moduleName = "Import Hold Records";
            }
            else if (jobType == (int)JobType.ImportWorkspaceHold)
            {
                moduleName = "Import Workspace Hold";
            }
            else if (jobType == (int)JobType.PhysicalMoveDataJob)
            {
                moduleName = "Physical request move";
            }
            else if (jobType >= 5000)
            {
                moduleName = ((JobType)jobType).ToString();
            }
            else
            {
                moduleName = "Default";
            }
            stringBuild.Append(moduleName);
            stringBuild.Append(Path.DirectorySeparatorChar);
            stringBuild.Append(jobId);
            stringBuild.Append(expandedName);

            return stringBuild.ToString();
        }
        private static string AssembleReportPathAfterHalf(string planId, string jobId, int category, string expandedName)
        {
            StringBuilder stringBuild = new StringBuilder();

            string moduleName = GetArchiverJobModuleName(category);

            stringBuild.Append(moduleName);
            stringBuild.Append(Path.DirectorySeparatorChar);

            if (planId != null && !planId.Equals(string.Empty))
            {
                stringBuild.Append(planId);
                stringBuild.Append(Path.DirectorySeparatorChar);
            }
            else
            {
                stringBuild.Append(moduleName + " Plan");
                stringBuild.Append(Path.DirectorySeparatorChar);
            }

            stringBuild.Append(jobId);
            stringBuild.Append(expandedName);

            return stringBuild.ToString();
        }

        public static string GetArchiverJobModuleName(int category)
        {
            PlanCategory cat = Enumer.Parse<PlanCategory>(category);
            if (cat != PlanCategory.None)
            {
                return Enum.GetName(typeof(PlanCategory), cat);
            }
            return "Others";
        }

        public static string GetArchiverJobReportUri(string planId, string jobId, int category, string expandedName)
        {
            //string tenantIdentity = CallContext.LogicalGetData("TenantIdentity") as string;
            logger.Debug("tenantIdentity: {0}", TenantLocalValue.LogonGroupId);
            return SecurityUtils.SafeCombinePath(AssembleReportPathAfterHalf(planId, jobId, category, expandedName));
        }

        public static string GetJobReportUri(string jobId, int jobType, string expandedName)
        {
            //string tenantIdentity = CallContext.LogicalGetData("TenantIdentity") as string;
            logger.Debug("tenantIdentity: {0}", TenantLocalValue.LogonGroupId);
            return SecurityUtils.SafeCombinePath(GetTenantIdentity(), AssembleReportPathAfterHalf(jobId, jobType, expandedName));
        }

        //由于physicaltemplate.csv存在于包中，可以不用上传至blob
        public static void UploadCSVTemplateToBlob()
        {
            try
            {
                string templateName = "physicaltemplate.csv";
                var blobName = SecurityUtils.SafeCombinePath(TemplateFolder, templateName);
                RAStorageUtil.UploadReportBlob(blobName, SecurityUtils.SafeCombinePath(WebUtil.GetInstallPath(), "Config", templateName));
            }
            catch (Exception e)
            {
                logger.Error("uploading physicaltemplate.csv error:{0}", e.ToString());
            }
        }
        //由于physicaltemplate.csv存在于包中，可以不用上传至blob
        public static string DownloadCSVTemplateToFile()
        {
            try
            {
                string templateName = "physicaltemplate.csv";
                var blobName = SecurityUtils.SafeCombinePath(TemplateFolder, templateName);
                var downloadPath = SecurityUtils.SafeCombinePath(RMGlobalConfiguration.StorageConfig[Contract.Configurations.RMStorageSettingKey.REPORT_TEMP_FOLDER], TemplateFolder, templateName);
                if (!Directory.Exists(Path.GetDirectoryName(downloadPath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(downloadPath));
                }
                RAStorageUtil.DownloadReportBlobToFile(blobName, downloadPath);
                return downloadPath;
            }
            catch (Exception e)
            {
                logger.Error("downloading physicaltemplate.csv error:{0}", e.ToString());
                return null;
            }
        }

        public static string GetImportJobCSVFile(string blobFileName)
        {
            blobFileName = TrimQuotMark(blobFileName);
            string downloadToFilename = SecurityUtils.SafeCombinePath(RMGlobalConfiguration.StorageConfig[Contract.Configurations.RMStorageSettingKey.REPORT_TEMP_FOLDER], GetTenantIdentity(), ImportCSVFile, Path.GetFileName(blobFileName));
            var downloadFolder = Path.GetDirectoryName(downloadToFilename);
            if (!Directory.Exists(downloadFolder)) { Directory.CreateDirectory(downloadFolder); }
            RAStorageUtil.DownloadReportBlobToFile(blobFileName, downloadToFilename);
            RAStorageUtil.DeleteReportBlob(blobFileName);
            return downloadToFilename;
        }
        public static string GetFullTextIndexJobFile(string blobFileName)
        {
            blobFileName = TrimQuotMark(blobFileName);
            string downloadToFilename = SecurityUtils.SafeCombinePath(RMGlobalConfiguration.StorageConfig[Contract.Configurations.RMStorageSettingKey.REPORT_TEMP_FOLDER], GetTenantIdentity(), FullTextIndexJobInfoFile, Path.GetFileName(blobFileName));
            var downloadFolder = Path.GetDirectoryName(downloadToFilename);
            if (!Directory.Exists(downloadFolder)) { Directory.CreateDirectory(downloadFolder); }
            RAStorageUtil.DownloadReportBlobToFile(blobFileName, downloadToFilename);
            RAStorageUtil.DeleteReportBlob(blobFileName);
            return downloadToFilename;
        }
        public static string GetImportJobMetaFileWithoutDeletion(string blobFileName)
        {
            blobFileName = TrimQuotMark(blobFileName);
            string downloadToFilename = SecurityUtils.SafeCombinePath(RMGlobalConfiguration.StorageConfig[Contract.Configurations.RMStorageSettingKey.REPORT_TEMP_FOLDER], GetTenantIdentity(), ImportCSVFile, Path.GetFileName(blobFileName));
            var downloadFolder = Path.GetDirectoryName(downloadToFilename);
            if (!Directory.Exists(downloadFolder)) { Directory.CreateDirectory(downloadFolder); }
            RAStorageUtil.DownloadReportBlobToFile(blobFileName, downloadToFilename);
            return downloadToFilename;
        }
        private static string TrimQuotMark(string temp)
        {
            if (temp != null)
            {
                return temp.Trim('"');
            }
            return temp;
        }

        public static string GetSearchResultFilePath(string filename)
        {
            return SecurityUtils.SafeCombinePath(
                RMGlobalConfiguration.StorageConfig[Contract.Configurations.RMStorageSettingKey.REPORT_TEMP_FOLDER], GetTenantIdentity(), filename);
        }
        public static string GetSearchResultBlobPath(string filename)
        {
            StringBuilder stringBuild = new StringBuilder();
            stringBuild.Append(SecurityUtils.SafeCombinePath(GetTenantIdentity(), "OfflineSearchResult")).Append(Path.DirectorySeparatorChar);
            stringBuild.Append(filename);
            return stringBuild.ToString();
        }
        public static string GetUploadExportConfigZipPath(string fileName)
        {
            return SecurityUtils.SafeCombinePath(Path.GetTempPath(), GetTenantIdentity(), ExportVEOConfig, fileName);
        }
        public static string GetSavedExportConfigPath()
        {
            return SecurityUtils.SafeCombinePath(Path.GetTempPath(), GetTenantIdentity(), ExportVEOConfig, "Saved");
        }

        public static string GetJobIdByPrefix(string prefix)
        {
            return string.Format("{0}{1}{2}", prefix, DateTime.Now.ToString("yyyyMMddHHmmss"), GenerateRandomNumber(6));
        }

        private static string GenerateRandomNumber(int count)
        {
            Random ran = new Random((int)DateTime.Now.Ticks);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < count; i++)
            {
                /* Fortify Issue Type: Insecure Randomness 
                * Sink Details: this class GetJobIdByPrefix 
                * Ignore Reason: random用于生成jobid，不涉及安全问题
                */
                sb.Append(ran.Next(0, 9)).ToString();
            }
            return sb.ToString();
        }
        public static string GetColumnByI18N(string key)
        {
            return !string.IsNullOrEmpty(key) ? key.StartsWith(I18NSTR) ? I18NEntity.GetString(key) : key : key;
        }

        /// <summary>
        /// i18n RM_SPS_Location_RootNode -> My Registered Locations
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string ReplaceRootLocationName(string path)
        {
            return I18NEntity.ReplaceI18NKey(path, "RM_", new string[] { "/" });
        }

        public static RMReportObjectLevel ConvertDaoOrOpusLevelToObjectLevel(string type)
        {
            RMReportObjectLevel res = RMReportObjectLevel.None;
            if (type.Equals("Site Collection", StringComparison.OrdinalIgnoreCase) || type.Equals("SiteCollection", StringComparison.OrdinalIgnoreCase)
                || type.Equals("RM_JS_Rule_ObjectLevel_SiteCollection", StringComparison.OrdinalIgnoreCase))
            {
                res = RMReportObjectLevel.SiteCollection;
            }
            else if (type.Equals("Site", StringComparison.OrdinalIgnoreCase) || type.Equals("RM_JS_Rule_ObjectLevel_Site", StringComparison.OrdinalIgnoreCase))
            {
                res = RMReportObjectLevel.Site;
            }
            else if (type.Equals("List", StringComparison.OrdinalIgnoreCase) || type.Equals("RM_JS_Rule_ObjectLevel_List", StringComparison.OrdinalIgnoreCase))
            {
                res = RMReportObjectLevel.List;
            }
            else if (type.Equals("Folder", StringComparison.OrdinalIgnoreCase) || type.Equals("RM_JS_Rule_ObjectLevel_Folder", StringComparison.OrdinalIgnoreCase))
            {
                res = RMReportObjectLevel.Folder;
            }
            else if (type.Equals("Item", StringComparison.OrdinalIgnoreCase) || type.Equals("RM_JS_Rule_ObjectLevel_Item", StringComparison.OrdinalIgnoreCase))
            {
                res = RMReportObjectLevel.Item;
            }
            else if (type.Equals("Attachment", StringComparison.OrdinalIgnoreCase) || type.Equals("RM_JS_Rule_ObjectLevel_Attachment", StringComparison.OrdinalIgnoreCase))
            {
                res = RMReportObjectLevel.Attachment;
            }
            else if (type.Equals("ItemVersion", StringComparison.OrdinalIgnoreCase) || type.Equals("RM_JS_Rule_ObjectLevel_ItemVersion", StringComparison.OrdinalIgnoreCase))
            {
                res = RMReportObjectLevel.ItemVersion;
            }
            else if (type.Equals("Document", StringComparison.OrdinalIgnoreCase) || type.Equals("RM_JS_Rule_ObjectLevel_Document", StringComparison.OrdinalIgnoreCase))
            {
                res = RMReportObjectLevel.Document;
            }
            else if (type.Equals("RM_JS_Rule_ObjectLevel_DocumentVersion", StringComparison.OrdinalIgnoreCase))
            {
                res = RMReportObjectLevel.DocumentVersion;
            }
            else if (type.Equals("RM_JS_Common_ReportType_GoogleDrive", StringComparison.OrdinalIgnoreCase))
            {
                res = RMReportObjectLevel.GoogleDrive;
            }
            else if (type.Equals("RM_JS_Rule_ObjectLevel_GoogleFolder", StringComparison.OrdinalIgnoreCase))
            {
                res = RMReportObjectLevel.GoogleFolder;
            }
            else if (type.Equals("RM_JS_Rule_ObjectLevel_GoogleFile", StringComparison.OrdinalIgnoreCase))
            {
                res = RMReportObjectLevel.GoogleFile;
            }
            return res;
        }

        public static string ConvertItemTypeStringForEXODetails(string itemType)
        {
            switch (itemType)
            {
                case "ExchangeOnlineMailbox"://处理老数据逻辑
                    itemType = I18NEntity.GetString("RM_EXO_LevelType_ExchangeOnlineMailbox");
                    break;
                case "ExchangeOnlineFolder"://处理老数据逻辑
                case "ExchangeFolder"://有些老数据使用的是这个Level
                    itemType = I18NEntity.GetString("RM_EXO_LevelType_ExchangeOnlineFolder");
                    break;
                case "ExchangeOnlineItem"://处理老数据逻辑
                    itemType = I18NEntity.GetString("RM_EXO_LevelType_ExchangeOnlineItem");
                    break;
                default:
                    itemType = I18NEntity.GetString(itemType);
                    break;
            }
            return itemType;
        }

        public static string ConvertStringForDetails(string itemType, bool isMergeRpt)
        {
            if (!isMergeRpt)
            {
                itemType = I18NEntity.GetString(itemType);
            }
            return itemType;
        }

        public static string ConvertItemTypeForDetails(NodeLevel nodeLevel)
        {
            string i18nKey = string.Empty;
            switch (nodeLevel)
            {
                case NodeLevel.ExchangeOnlineMailbox:
                    i18nKey = "RM_EXO_LevelType_ExchangeOnlineMailbox";
                    break;
                case NodeLevel.ExchangeOnlineFolder:
                case NodeLevel.ExchangeFolder://有些老数据使用的是这个Level
                    i18nKey = "RM_EXO_LevelType_ExchangeOnlineFolder";
                    break;
                case NodeLevel.ExchangeOnlineItem:
                    i18nKey = "RM_EXO_LevelType_ExchangeOnlineItem";
                    break;
                case NodeLevel.SiteCollection:
                    i18nKey = "RM_JS_Rule_ObjectLevel_SiteCollection";
                    break;
                case NodeLevel.Site:
                    i18nKey = "RM_JS_Rule_ObjectLevel_Site";
                    break;
                case NodeLevel.List:
                case NodeLevel.Library:
                    i18nKey = "RM_Common_ObjectLevel_List";
                    break;
                case NodeLevel.Folder:
                case NodeLevel.RootFolder:
                    i18nKey = "RM_Common_ObjectLevel_Folder";
                    break;
                case NodeLevel.Item:
                case NodeLevel.Document:
                    i18nKey = "RM_JS_Rule_ObjectLevel_Item";
                    break;
                default:
                    i18nKey = nodeLevel.ToString();
                    break;
            }
            return i18nKey;
        }
        public static string ConverTypeToLevel(string type)
        {
            switch (type)
            {
                case "E":
                    return "RM_JS_Rule_ObjectLevel_SiteCollection";
                case "W":
                    return "RM_JS_Rule_ObjectLevel_Site";
                case "L":
                    return "RM_JS_Rule_ObjectLevel_List";
                case "F":
                    return "RM_JS_Rule_ObjectLevel_Folder";
                case "D":
                case "I":
                    return "RM_JS_Rule_ObjectLevel_Item";
                case "A":
                    return "RM_JS_Rule_ObjectLevel_Attachment";
                case "Y":
                    return "RM_JS_Rule_ObjectLevel_App";
                default:
                    return type;
            }
        }
        public static string ConvertTypeToLevel(int type)
        {
            switch (type)
            {
                case (int)GDriveDataType.MyDrive:
                case (int)GDriveDataType.SharedDrive:
                    return "RM_JS_Common_ReportType_GoogleDrive";
                case (int)GDriveDataType.Folder:
                    return "RM_JS_Rule_ObjectLevel_GoogleFolder";
                case (int)GDriveDataType.File:
                    return "RM_JS_Rule_ObjectLevel_GoogleFile";
                case (int)GDriveDataType.FileVersion:
                    return "RM_JS_Rule_ObjectLevel_GoogleDriveFileVersion";
                default:
                    return type.ToString();
            }
        }

        public static CacheNodeType ConverTypeToNodeLevel(string type)
        {
            switch (type)
            {
                case "E":
                    return CacheNodeType.SiteCollection;
                case "W":
                    return CacheNodeType.Web;
                case "L":
                    return CacheNodeType.List;
                case "F":
                    return CacheNodeType.Folder;
                case "D":
                case "I":
                    return CacheNodeType.Item;
                case "A":
                    return CacheNodeType.Attachment;
                case "Y":
                    return CacheNodeType.APP;
                default:
                    return CacheNodeType.Exception;
            }
        }

        public static ProgressStatus ConvertJobStatusToProgressStatus(JobStatus jobStatus)
        {
            return jobStatus switch
            {
                JobStatus.Wait => ProgressStatus.Pending,
                JobStatus.Finished => ProgressStatus.Finished,
                JobStatus.Failed => ProgressStatus.Failed,
                JobStatus.FinishWithException => ProgressStatus.FinishWithException,
                JobStatus.Stopped => ProgressStatus.Stopped,
                JobStatus.Skipped => ProgressStatus.Skipped,
                _ => ProgressStatus.Finished,
            };
        }

        public static string GetDownloadsSiteCollectionBlacklistTempleFolder(string subFolder)
        {
            StringBuilder stringBuild = new StringBuilder();
            stringBuild.Append(AssembleReportPathForeBody());
            stringBuild.Append(ImportSCBlacklistFolder).Append(Path.DirectorySeparatorChar);
            stringBuild.Append(subFolder);
            return stringBuild.ToString();
        }



    }

    public class CheckJobStatusUtility
    {
        private static RALogger logger = RALogger.GetInstance(typeof(CheckJobStatusUtility));
        public static volatile bool isStopping = false;
        public static volatile bool jobIsFinished = false;

        public static bool NeedSimulate429ForQATest = false;
        public static DateTime lastCheckNeedSimulate429ForQATestTime = DateTime.MinValue;

        static CheckJobStatusUtility()
        {
            ReliableHttpWebRequest.CheckJobNeedStopEvent += ThrowExceptionIfJobNeedStop;
        }

        public static void ThrowExceptionIfJobNeedStop()
        {
            if (isStopping || jobIsFinished)
            {
                throw new Contract.Global.Exceptions.JobStopException();
            }
        }


        public static void Start(string jobId)
        {
            logger.Info("Start Checking Job Is Stopping ,ID:{0}", jobId);
            isStopping = false;
            jobIsFinished = false;
            AveTenantThread checkThread = new AveTenantThread(new ParameterizedThreadStart(CheckJobIsStopping));
            checkThread.IsBackground = true;
            checkThread.Start(jobId);
        }

        public static void CheckJobIsStopping(object id)
        {
            IJobMonitorService jService = (IJobMonitorService)PlatformWindsorManager.GetService(typeof(IJobMonitorService));
            int intervalTime = 1000 * 30;//30S
            string jobId = id as string;
            while (!isStopping && !jobIsFinished)
            {
                JobStatus jobStatus = jService.GetJobStatus(jobId);
                if (jobStatus.Equals(JobStatus.Finished) || jobStatus.Equals(JobStatus.Failed) || jobStatus.Equals(JobStatus.FinishWithException) || jobStatus.Equals(JobStatus.Skipped))
                {
                    logger.Info($"This Job is Finished.ID:{jobId}.JobStatus:{jobStatus}.");
                    jobIsFinished = true;
                }
                if (!jobIsFinished)
                {
                    if (jobStatus.Equals(JobStatus.Stopping) || jobStatus.Equals(JobStatus.Stopped))
                    {
                        isStopping = true;
                        logger.Info("This Job Is Stopped ,ID:{0}", jobId);
                    }
                    else
                    {
                        logger.Info($"Start Checking Job Is Stopping.ID:{jobId}.JobStatus:{jobStatus}.isStopping:{isStopping}.jobIsFinished:{jobIsFinished}.memory use:{ProcessUtil.GetProcessMemoryMB()} MB");
                    }
                    //if (isStopping)
                    //{
                    //    logger.Info("This Job Is Stopped ,ID:{0}", jobId);
                    //}
                    //else
                    //{
                    //    //logger.Info("Start Checking Job Is Stopping ,ID:{0}", jobId);
                    //}
                    Thread.Sleep(intervalTime);
                }
            }
        }

    }
    public class CheckJobStopScope : IDisposable
    {
        public CheckJobStopScope()
        {
            if (CheckJobStatusUtility.isStopping)
            {
                throw new JobStopException("This Job is stopped.");
            }
        }
        public void Dispose()
        {
            if (CheckJobStatusUtility.isStopping)
            {
                throw new JobStopException("This Job is stopped.");
            }
        }
    }
}
