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
using AvePoint.GCommon.Contract.CloudAppAdmin.Object;
using AvePoint.GCommon.Contract.Media.Object.Common;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.SharePoint.ArchiverCommon;
using Media.Service.ArchiverBackup.Statistics;
using Newtonsoft.Json;
using RAArchiverCommon;
using RAGoogle.Archive;
using RAGoogle.Archive.Common;
using RAGoogle.Common;
using RAGoogle.Extension;
using RAGoogle.Models;
using RAGoogle.RecordsDisposal.Action.Archive.Data;
using RAGoogle.Services;
using RAGoogle.Util;
using System.Xml;

namespace RAGoogle.RecordsDisposal.Action.DeleteOnly
{
    internal class DeleteOnlyController(GoogleConfiguration configuration, Record? record) : BaseBackupController(configuration)
    {
        #region properties

        private IRALogger _logger = RALogger.GetInstance(typeof(DeleteOnlyController));
        private ITermDao TermDao => PlatformWindsorManager.GetService<ITermDao>();

        private bool isArchiver = false;
        #endregion

        public override async Task Process(GoogleItemData item)
        {
            if (item.Level != RMNodeLevel.GoogleFile)
            {
                _logger.Info("Ignore item because node level is different from google file. ItemName: {0}", item.Name);
                return;
            }
            if (!isArchiver && mArchiveItem?.CacheNodeType == (int)GoogleCacheNodeType.ItemVersion)
            {
                _logger.Info($"Item File has been deleted so ignore Item version when action is not Archive. ItemName: {item.Name}, CacheType: {(GoogleCacheNodeType)mArchiveItem.CacheNodeType}");
                return;
            }
            try
            {
                using (PerformanceScope pc = new PerformanceScope("DeleteOnlyController.Process"));
                using GoogleDriveService service = new(mConfiguration.AppProfile, item.MemberEmail);
                using CheckJobStopScope jScope = new();
                if (string.IsNullOrEmpty(item.Id))
                {
                    _logger.Error($"Id Item Empty : Item name {item.Name}");
                    return;
                }
                _logger.Info($"Delete Google item [{item.Id}]");
                if (await service.TryDeleteItemById(item.Id))
                {
                    SaveDataToLiteDB(item, record);
                    if (record != null)
                    {
                        _logger.Info($"Update record [{record.Id}]");
                        //var isArchiveAction = mConfiguration.Action == Models.Enums.ActionType.ArchiveToStorage || mConfiguration.Action == Models.Enums.ActionType.ExportBeforeArchive;
                        mConfiguration.RecordManager?.UpdateRecordStatusAndDestroyedTime(record, mConfiguration.CurrentRule, /*isArchiveAction ? (int)RMRecordStatus.Archived : */(int)RMRecordStatus.Destroyed);
                    }
                    if (!isArchiver)
                    {
                        SOGDriveArchiverJobInfoStatistics.Instance.AccumulationItemsSize(item.Size ?? 0 , item.Name);
                        SOGDriveArchiverJobInfoStatistics.Instance.AccumulationDeletedNumber();
                    }
                   
                    item.AddToOtherSummaryReportsByGoogleItem(mConfiguration.ActionApproveReports, JobDetailsStatus.Successful, mConfiguration.CurrentRule?.Name, string.Empty, I18NResource.RemoveAndDestroyAction);
                }
            }
            catch (JobStopException)
            {
                _logger.Warn("the job has stopped.");
                throw new JobStopException("the job has stopped");
            }
            catch (Exception ex)
            {
                string message = I18NResource.DeleteItemFailed;
                if (ex.Message.Contains(I18NResource.InvalidUserPermission))
                {
                    message = I18NResource.InvalidUserPermission;
                }
                _logger.Error($"An error occurred while deleting item [{item.Id}]. Error: {ex}");
                if (SOArchiverJobInfoStatistics.Instance.DriveAndDeleteSize == null)
                {
                    SOArchiverJobInfoStatistics.Instance.DriveAndDeleteSize = new();
                }

                if (!SOArchiverJobInfoStatistics.Instance.DriveAndDeleteSize.TryGetValue(item.DriveName, out var size))
                {
                    size += item.Size!.Value;
                }
                item.AddToOtherSummaryReportsByGoogleItem(mConfiguration.ActionApproveReports, JobDetailsStatus.Failed, mConfiguration.CurrentRule?.Name, message, I18NResource.RemoveAndDestroyAction);
            }
        }
        public override async Task ProcessArchiveReport(ArchiveApproveReport item, BackupNodeParameters nodeParameters)
        {
            mArchiveItem = item;
            isArchiver = false;
            if (item.JsonMeta.IsNotNullOrEmpty())
            {
                mGoogleItem = JsonConvert.DeserializeObject<GoogleItemData>(item.JsonMeta) ?? new();
            }
            if (mGoogleItem != null)
            {
                if (mArchiveItem.TermId.IsNotNullOrEmpty() && mConfiguration.CurrentTerm == null)
                {
                    mConfiguration.CurrentTerm = TermDao.GetActiveTermById(Convert.ToInt32(mArchiveItem.TermId));
                }

                mConfiguration.RecordManager.TryGetRecordValue(mGoogleItem.UniqueId, 0, out record);
                await Process(mGoogleItem);
            }
        }

        internal async Task ProcessStringItemAsync(string item)
        {
            if (ConvertStringToItem(item))
            {
                isArchiver = true;
                mConfiguration.RecordManager.TryGetRecordValue(mGoogleItem.UniqueId, 0, out record);
                await Process(mGoogleItem);
            }
        }
        private bool ConvertStringToItem(String fileHeaderXml)
        {
            var docment = new XmlDocument();
            try
            {
                docment.LoadXml(fileHeaderXml);
                mGoogleItem = new GoogleItemData()
                {
                    Level = RMNodeLevel.GoogleFile,
                };
                var rootElement = docment.DocumentElement;
                if (rootElement.HasAttribute(GDriveKeyWord.Name))
                {
                    mGoogleItem.Name = rootElement.GetAttribute(GDriveKeyWord.Name);
                }
                if (rootElement.HasAttribute(GDriveKeyWord.ParentId))
                {
                    mGoogleItem.ParentId = rootElement.GetAttribute(GDriveKeyWord.ParentId);
                }
                if (rootElement.HasAttribute(GDriveKeyWord.ParentIds))
                {
                    mGoogleItem.ParentIds = rootElement.GetAttribute(GDriveKeyWord.ParentIds);
                }
                if (rootElement.HasAttribute(GDriveKeyWord.Path))
                {
                    mGoogleItem.Path = rootElement.GetAttribute(GDriveKeyWord.Path);
                }
                if (rootElement.HasAttribute(GDriveKeyWord.CreatedTime))
                {
                    mGoogleItem.CreatedTime = new DateTime(Convert.ToInt64(rootElement.GetAttribute(GDriveKeyWord.CreatedTime)));
                }
                if (rootElement.HasAttribute(GDriveKeyWord.ModifiedTime))
                {
                    mGoogleItem.ModifiedTime = new DateTime(Convert.ToInt64(rootElement.GetAttribute(GDriveKeyWord.ModifiedTime)));
                }
                if (rootElement.HasAttribute(GDriveKeyWord.CreatedBy))
                {
                    mGoogleItem.CreatedBy = rootElement.GetAttribute(GDriveKeyWord.CreatedBy);
                }
                if (rootElement.HasAttribute(GDriveKeyWord.DriveId))
                {
                    mGoogleItem.DriveId = rootElement.GetAttribute(GDriveKeyWord.DriveId);
                }
                if (rootElement.HasAttribute(GDriveKeyWord.DriveName))
                {
                    mGoogleItem.DriveName = rootElement.GetAttribute(GDriveKeyWord.DriveName);
                }
                if (rootElement.HasAttribute(GDriveKeyWord.MemberEmail))
                {
                    mGoogleItem.MemberEmail = rootElement.GetAttribute(GDriveKeyWord.MemberEmail);
                }
                if (rootElement.HasAttribute(GDriveKeyWord.ItemId))
                {
                    mGoogleItem.Id = rootElement.GetAttribute(GDriveKeyWord.ItemId);
                }
                if (rootElement.HasAttribute(GDriveKeyWord.Size))
                {
                    mGoogleItem.Size = long.Parse(rootElement.GetAttribute(GDriveKeyWord.Size));
                }
                if (rootElement.HasAttribute(GDriveKeyWord.Path))
                {
                    mGoogleItem.RelativePath = rootElement.GetAttribute(GDriveKeyWord.Path);
                }
                if (rootElement.HasAttribute(GDriveKeyWord.FileExtension))
                {
                    mGoogleItem.FileExtension = rootElement.GetAttribute(GDriveKeyWord.FileExtension);
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to convert string to GoogleItemData. Exception: {ex}");
                return false;
            }
            finally
            {
                docment.RemoveAll();
            }
            return true;
        }
    }
}
