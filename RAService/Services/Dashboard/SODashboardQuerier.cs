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
using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.Dashboard;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.JobQueue;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Dashboard
{
    public class SODashboardQuerier
    {

        private static IRMArchiveSiteInfoDao RMArchiveSiteInfoDao => PlatformWindsorManager.GetService<IRMArchiveSiteInfoDao>();
        private static IRMArchiveGDriveInfoDao RMArchiveGDInfoDao => PlatformWindsorManager.GetService<IRMArchiveGDriveInfoDao>();
        private static IRMArchiveTeamsGroupInfoDao RMArchiveTeamsGroupInfoDao => PlatformWindsorManager.GetService<IRMArchiveTeamsGroupInfoDao>();

        private static IRMRetentionSimulateInfosDao RetentionSimulateInfosDao => PlatformWindsorManager.GetService<IRMRetentionSimulateInfosDao>();
        private static readonly IGeneralSettingService GeneralSettingService = PlatformWindsorManager.GetService<IGeneralSettingService>();
        private static IDashboardService DashboardService => PlatformWindsorManager.GetService<IDashboardService>();

        public static async Task<ArchiverDataDetails> GetArchiverDataSizeAsync()
        {
            double archivedTotalSize = 0;
            try
            {
                archivedTotalSize += await RMArchiveSiteInfoDao.GetArchiverDataSizeAsync();
                archivedTotalSize += await RMArchiveTeamsGroupInfoDao.GetArchivedSizeWithoutRelatedSitesAsync();
            }
            catch
            {
                if(await RMArchiveSiteInfoDao.ExistArchvierData())
                {
                    throw;
                }
            }

            var unit = ArchiverDataUnit.GB;
            if (archivedTotalSize >= 10000)
            {
                archivedTotalSize /= 1024;
                unit = ArchiverDataUnit.TB;
            }
            return new ArchiverDataDetails()
            {
                TotalSize = archivedTotalSize < 0.005 ? "0" : archivedTotalSize.ToString("F2"),
                ArchiverDataUnit = archivedTotalSize < 0.005 ? ArchiverDataUnit.GB : unit,
            };
        }

        public static async Task<ArchiverDataDetails> GetDeleteDataSizeAsync()
        {
            double archivedTotalSize = 0;
            try
            {
                archivedTotalSize += await RMArchiveSiteInfoDao.GetDeleteSizeAsync();
            }
            catch
            {
                if (await RMArchiveSiteInfoDao.ExistArchvierData())
                {
                    throw;
                }
            }

            var unit = ArchiverDataUnit.GB;
            if (archivedTotalSize >= 10000)
            {
                archivedTotalSize /= 1024;
                unit = ArchiverDataUnit.TB;
            }
            return new ArchiverDataDetails()
            {
                TotalSize = archivedTotalSize < 0.005 ? "0" : archivedTotalSize.ToString("F2"),
                ArchiverDataUnit = archivedTotalSize < 0.005 ? ArchiverDataUnit.GB : unit,
            };
        }

        public static async Task<SOSummaryTotalDataDetails> GetSOTotalDataDetailsAsync()
        {
            var archiverSizeInfo = await GetArchiverDataSizeAsync();
            var archiverFileInfo = await GetArchiverFileCountAsync();
            var deletedSizeInfo = await GetDeleteDataSizeAsync();
            var deletedFileCountInfo = await GetDeleteFileCountAsync();
            return new SOSummaryTotalDataDetails
            {
                ArchiverTotalSize = archiverSizeInfo.TotalSize,
                ArchiverDataSizeUnit = archiverSizeInfo.ArchiverDataUnit,
                ArchiverTotalFileCount = archiverFileInfo.TotalSize,
                ArchiverFileCountUnit = archiverFileInfo.ArchiverDataUnit,
                DeleteTotalSize = deletedSizeInfo.TotalSize,
                DeleteDataSizeUnit = deletedSizeInfo.ArchiverDataUnit,
                DeleteTotalFileCount = deletedFileCountInfo.TotalSize,
                DeleteFileCountUnit = deletedFileCountInfo.ArchiverDataUnit,
            };
        }

        private static IJobDetailService JDService => PlatformWindsorManager.GetService<IJobDetailService>();
        private static IRMScheduleDao RMScheduleDao => PlatformWindsorManager.GetService<IRMScheduleDao>();


        private static long GetNextRetentionRunTime()
        {
            var schedule = RMScheduleDao.GetScheduleByType(AvePoint.RA.Contract.Schedule.ScheduleType.ArchiveDataRetentionSchedule);
            if (schedule != null && schedule.Count > 0)
            {
                return schedule.FirstOrDefault().NextTime;
            }
            return 0;
        }


        public static async Task<string> GetArchiverRetentionSimulateDataDetails(JMDetailsQuery queryModel)
        {
            JMDetailsResult result = new JMDetailsResult() { Success = true };
            int totalCount = 0;

            var mainJob = RetentionSimulateInfosDao.GetAll().FirstOrDefault(r => r.SourceFlag == (int)SourceFlag.All);
            if (mainJob == null || mainJob.MergeReportState != (int)MergeIndexState.Succeed)
            {
                return JsonConvert.SerializeObject(result);
            }

            var jobDto = new BaseJobDto()
            {
                Id = $"{mainJob.RetentionJobId}",
                JobType = (int)JobType.ArchiverRetentionSimulate,
                //AddValues = addValues
            };

            var data = JDService.GetDataForRetentionSimulateDetails(queryModel.PageSize, queryModel.CurrentPage, ref totalCount, "", jobDto);

            result.Details = data;

            result.TotalNumber = totalCount;

            return JsonConvert.SerializeObject(result);
        }

        public static async Task<ArchiverRetentionDataDetails> GetArchiverRetentionDataSizeAsync()
        {
            ArchiverRetentionDataDetails dataDetail = new ArchiverRetentionDataDetails()
            {
                DeleteTime = I18NEntity.GetString("RM_JS_Common_Pending")
            };
            double archivedTotalSize = 0;
            long fileNumber = 0;

            var mainJob = RetentionSimulateInfosDao.GetAll().FirstOrDefault(r => r.SourceFlag == (int)SourceFlag.All);
            if (mainJob == null || mainJob.MergeReportState != (int)MergeIndexState.Succeed)
            {
                return dataDetail;
            }

            long nextRunJobDate = mainJob.NextRunJobDate;
            long lastRunJobDate = mainJob.LastRunJobDate;

            var generalSetting = GeneralSettingService.GetGeneralSettingAsync().Result;

            dataDetail.RetentionJobId = mainJob.RetentionJobId;
            dataDetail.NextRunJobDate = GeneralSettingService.ConvertTiksToDateTime(generalSetting, nextRunJobDate, false).SimplifyFormatTime;
            dataDetail.LastRunJobDate = GeneralSettingService.ConvertTiksToDateTime(generalSetting, lastRunJobDate, false).SimplifyFormatTime;
            dataDetail.DeleteTime = GeneralSettingService.ConvertTiksToDateTime(generalSetting, nextRunJobDate, false).FormaDate;

            archivedTotalSize = mainJob.DataSize;
            fileNumber = mainJob.FileNumber;

            dataDetail.TotalSize = FormatSize(archivedTotalSize, out ArchiverDataUnit sizeUnite);
            dataDetail.TotalSizeDataUnit = sizeUnite;

            dataDetail.TotalNumber = FormatFileCount(fileNumber, out ArchiverDataUnit fileUnite);
            dataDetail.TotalNumberDataUnit = fileUnite;

            return dataDetail;
        }

        public static string FormatFileCount(double count, out ArchiverDataUnit unite)
        {
            double value;
            if (count >= 1_000_000)
            {
                unite = ArchiverDataUnit.Million;
                value = count / 1_000_000d;
            }
            else
            {
                unite = ArchiverDataUnit.K;
                value = count / 1_000d;
            }

            const double epsilon = 1e-10;

            // Check if value is approximately zero using a range
            if (Math.Abs(value) < epsilon)
            {
                return "0";
            }
            if (value < 0.01)
            {
                return "<0.01";
            }
            return value.ToString("F2");
        }

        public static string FormatSize(double bytes, out ArchiverDataUnit unite)
        {
            const double bytesPerGB = 1024 * 1024 * 1024;
            const double bytesPerTB = 1024 * bytesPerGB;

            double value;
            if (bytes >= bytesPerTB)
            {
                unite = ArchiverDataUnit.TB;
                value = bytes / bytesPerTB;
            }
            else
            {
                unite = ArchiverDataUnit.GB;
                value = bytes / bytesPerGB;
            }

            const double epsilon = 1e-10;

            // Check if value is approximately zero using a range
            if (Math.Abs(value) < epsilon)
            {
                return "0";
            }
            if (value < 0.01)
            {
                return "<0.01";
            }
            return value.ToString("F2");
        }

        public static async Task<ArchiverDataDetails> GetArchiverFileCountAsync()
        {
            var fileTotalCount = await RMArchiveSiteInfoDao.GetArchiverFileCountAsync();
            var unit = ArchiverDataUnit.K;
            if (fileTotalCount >= 2 * 1000)
            {
                fileTotalCount /= 1000;
                unit = ArchiverDataUnit.Million;
            }
            return new ArchiverDataDetails()
            {
                TotalSize = fileTotalCount < 0.005 ? "0" : fileTotalCount.ToString("F2"),
                ArchiverDataUnit = unit,
            };
        }

        public static async Task<ArchiverDataDetails> GetDeleteFileCountAsync()
        {
            var fileTotalCount = await RMArchiveSiteInfoDao.GetDeleteFileCountAsync();
            var unit = ArchiverDataUnit.K;
            if (fileTotalCount >= 2 * 1000)
            {
                fileTotalCount /= 1000;
                unit = ArchiverDataUnit.Million;
            }
            return new ArchiverDataDetails()
            {
                TotalSize = fileTotalCount < 0.005 ? "0" : fileTotalCount.ToString("F2"),
                ArchiverDataUnit = unit,
            };
        }

        public static async Task<ArchiverDataDetails> GetArchiverVersionCountAsync()
        {
            var versionTotalCount = await RMArchiveSiteInfoDao.GetArchiverVersionCountAsync();
            var unit = ArchiverDataUnit.K;
            if (versionTotalCount >= 2 * 1000)
            {
                versionTotalCount /= 1000;
                unit = ArchiverDataUnit.Million;
            }
            return new ArchiverDataDetails()
            {
                TotalSize = versionTotalCount < 0.005 ? "0" : versionTotalCount.ToString("F2"),
                ArchiverDataUnit = unit,
            };
        }

        public static async Task<List<ArchiverSiteSizeInfo>> GetArchiverTop50SitesAsync()
        {
            var result = await RMArchiveSiteInfoDao.GetArchiverTop50SitesAsync();
            return result.Select(site => new ArchiverSiteSizeInfo()
            {
                SiteUrl = site.SiteUrl,
                TotalSize = site.ArchivedSize < 0.005 && site.ArchivedSize > 0 ? I18NEntity.GetString("RM_DSB_Unit_LessThan") : site.ArchivedSize.ToString("F2"),
                TotalDeleteSize = site.DeletedSize < 0.005 && site.DeletedSize > 0 ? I18NEntity.GetString("RM_DSB_Unit_LessThan") : site.DeletedSize.ToString("F2"),
                TotalSizeArchivedByM365 = site.ArchiveBy365Size < 0.005 ? I18NEntity.GetString("RM_DSB_Unit_LessThan") : site.ArchiveBy365Size.ToString("F2"),
            }).ToList();
        }       
        
        public static async Task<ArchiverSiteSizeInfoWithCount> GetArchiverSitesByPagerAsync(ArchiverSitePageMode queryMode)
        {
            var count = await RMArchiveSiteInfoDao.GetArchiverSitesTotalCount4DashboardAsync(queryMode.SearchKey);
            var result = await RMArchiveSiteInfoDao.GetArchiverSitesByPagerAsync(queryMode.PageIndex + 1, queryMode.PageSize, queryMode.SearchKey);
            var siteInfos = result.Select(site => new ArchiverSiteSizeInfo()
            {
                SiteUrl = site.SiteUrl,
                TotalSize = site.ArchivedSize < 0.005 ? I18NEntity.GetString("RM_DSB_Unit_LessThan") : site.ArchivedSize.ToString("F2"),
                TotalDeleteSize = site.DeletedSize < 0.005 ? I18NEntity.GetString("RM_DSB_Unit_LessThan") : site.DeletedSize.ToString("F2"),
                TotalSizeArchivedByM365 = site.ArchiveBy365Size < 0.005 ? I18NEntity.GetString("RM_DSB_Unit_LessThan") : site.ArchiveBy365Size.ToString("F2"),
                SiteId = site.SiteId,
            }).ToList();
            return new ArchiverSiteSizeInfoWithCount()
            {
                ArchiverSiteSizeInfos = siteInfos,
                Count = count,
            };
        }

        public static async Task<ArchiverSiteSizeInfoWithCount> GetGoogleArchiverByPagerAsync(ArchiverSitePageMode queryMode)
        {
            var count = await RMArchiveGDInfoDao.GetGoogleArchiverTotalCount4DashboardAsync(queryMode.SearchKey);
            var result = await RMArchiveGDInfoDao.GetGoogleArchiverByPagerAsync(queryMode.PageIndex + 1, queryMode.PageSize, queryMode.SearchKey);
            var siteInfos = result.Select(site => new ArchiverSiteSizeInfo()
            {
                SiteUrl = site.DriveName,
                TotalSize = site.ArchivedSize < 0.005 ? I18NEntity.GetString("RM_DSB_Unit_LessThan") : site.ArchivedSize.ToString("F2"),
                TotalDeleteSize = site.DeletedSize < 0.005 ? I18NEntity.GetString("RM_DSB_Unit_LessThan") : site.DeletedSize.ToString("F2"),
                SiteId = site.DriveId,
            }).ToList();
            return new ArchiverSiteSizeInfoWithCount()
            {
                ArchiverSiteSizeInfos = siteInfos,
                Count = count,
            };
        }

        public static async Task<ArchiverSiteSizeInfoWithCount> GetArchiverSitesByPagerToExportAsync(ArchiverSitePageMode queryMode)
        {
            var count = await RMArchiveSiteInfoDao.GetArchiverSitesTotalCount4DashboardAsync();
            var result = await RMArchiveSiteInfoDao.GetArchiverSitesByPagerAsync(queryMode.PageIndex + 1, queryMode.PageSize);
            var siteInfos = result.Select(site => new ArchiverSiteSizeInfo()
            {
                SiteUrl = site.SiteUrl,
                TotalSize = site.ArchivedSize + "GB",
                SiteId = site.SiteId,
                TotalDeleteSize = site.DeletedSize + "GB",
            }).ToList();
            return new ArchiverSiteSizeInfoWithCount()
            {
                ArchiverSiteSizeInfos = siteInfos,
                Count = count,
            };
        }

        public static async Task<ArchiverDataDetails> GetYearlySavingAsync()
        {
            var config = await DashboardService.GetSOPriceConfigurationAsync();
            double costSaving = 0;
            var totalArchivedSize = await RMArchiveSiteInfoDao.GetArchiverDataSizeAsync();
            var totalTeamsArchivedSize = await RMArchiveTeamsGroupInfoDao.GetArchivedSizeWithoutRelatedSitesAsync();
            costSaving = Math.Round(totalArchivedSize , 2) * config.SharePointStoragePrice * 12 - Math.Round(totalArchivedSize + totalTeamsArchivedSize, 2) * config.ArchivedStoragePrice * 12;
            return new ArchiverDataDetails()
            {
                TotalSize = costSaving < 0.005 ? "0" : costSaving.ToString("F2"),
                ArchiverDataUnit = ArchiverDataUnit.Unknown,
            };
        }

        #region Teams
        public static async Task<List<ArchiverTeamsGroupSizeInfo>> GetArchiverTop50TeamsGroupsAsync()
        {
            var result = await RMArchiveTeamsGroupInfoDao.GetArchiverTop50TeamsGroupsAsync();
            return result.Select(site => new ArchiverTeamsGroupSizeInfo()
            {
                MailboxAddress = site.MailboxAddress,
                TotalArchivedSize = site.ArchivedSize < 0.005 ? I18NEntity.GetString("RM_DSB_Unit_LessThan") : site.ArchivedSize.ToString("F2"),
                TotalArchivedSizeWithoutRelatedSites = site.ArchivedSizeWithoutRelatedSites < 0.005 ? I18NEntity.GetString("RM_DSB_Unit_LessThan") : site.ArchivedSizeWithoutRelatedSites.ToString("F2"),
            }).ToList();
        }

        public static async Task<ArchiverTeamsGroupInfoWithCount> GetArchiverTeamsGroupByPagerAsync(ArchiverSitePageMode queryMode)
        {
            var count = await RMArchiveTeamsGroupInfoDao.GetArchiverTeamsGroupTotalCountAsync(queryMode.SearchKey);
            var result = await RMArchiveTeamsGroupInfoDao.GetArchiverTeamsGroupsByPagerAsync(queryMode.PageIndex + 1, queryMode.PageSize, queryMode.SearchKey);
            var teamsGroupInfoes = result.Select(teams => new ArchiverTeamsGroupSizeInfo()
            {
                MailboxAddress = teams.MailboxAddress,
                TotalArchivedSize = teams.ArchivedSize < 0.005 ? I18NEntity.GetString("RM_DSB_Unit_LessThan") : teams.ArchivedSize.ToString("F2"),
                TotalArchivedSizeWithoutRelatedSites = teams.ArchivedSizeWithoutRelatedSites < 0.005 ? I18NEntity.GetString("RM_DSB_Unit_LessThan") : teams.ArchivedSizeWithoutRelatedSites.ToString("F2"),
                //TeamsGroupId = teams.TeamsGroupId,
            }).ToList();
            return new ArchiverTeamsGroupInfoWithCount()
            {
                ArchiverTeamsGroupSizeInfoes = teamsGroupInfoes,
                Count = count,
            };
        }

        public static async Task<ArchiverTeamsGroupInfoWithCount> GetArchiverTeamsGroupByPagerToExportAsync(ArchiverSitePageMode queryMode)
        {
            var count = await RMArchiveTeamsGroupInfoDao.GetArchiverTeamsGroupTotalCountAsync();
            var result = await RMArchiveTeamsGroupInfoDao.GetArchiverTeamsGroupsByPagerAsync(queryMode.PageIndex + 1, queryMode.PageSize);
            var teamsGroupInfoes = result.Select(teams => new ArchiverTeamsGroupSizeInfo()
            {
                MailboxAddress = teams.MailboxAddress,
                TotalArchivedSize = teams.ArchivedSize + ArchiverDataUnit.GB.ToString(),
                TotalArchivedSizeWithoutRelatedSites = teams.ArchivedSizeWithoutRelatedSites + ArchiverDataUnit.GB.ToString(),
                //TeamsGroupId = teams.TeamsGroupId,
            }).ToList();
            return new ArchiverTeamsGroupInfoWithCount()
            {
                ArchiverTeamsGroupSizeInfoes = teamsGroupInfoes,
                Count = count,
            };
        }
        #endregion
    }
}
