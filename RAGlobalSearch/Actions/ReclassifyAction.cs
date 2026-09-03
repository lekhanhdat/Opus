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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object.RealTime;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.SharePoint.OneDrive.RMOneDriveExplorer;
using AvePoint.RA.SharePoint.RMExplorer.RMReclassifier;
using RACloudFS.FSFolderJob;
using RAGoogle.GoogleExplorer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAGlobalSearch.Actions
{
    public class ReclassifyAction : IGlobalSearchAction
    {
        private AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(ReclassifyAction));

        private IExplorerService mExplorerService;
        public IExplorerService ExplorerService
        {
            get
            {
                if (mExplorerService == null)
                {
                    mExplorerService = (IExplorerService)PlatformWindsorManager.GetService(typeof(IExplorerService));
                }
                return mExplorerService;
            }
        }
        private IExplorerDao _explorerDao;
        public IExplorerDao ExplorerDao
        {
            get
            {
                if (_explorerDao == null)
                {
                    _explorerDao = new ExplorerDao();
                }
                return _explorerDao;
            }
        }
        private int mFailedCount = 0;
        private int mSuccessCount = 0;
        private Hashtable mProcessedFSFolderId = new Hashtable();
        public ReclassifyAction()
        {
        }
        public async Task DoActionAsync(List<BaseRecordDto> records, SourceFlag flag, object actionExtension, string jobId, bool isJob)
        {
            logger.Info("Start process reclassify action.");
            try
            {
                if(flag == SourceFlag.Google)
                {
                    var folders = records.Where(r => r.NodeType == (int)RMNodeLevel.GoogleFolder).ToList();
                    var files = records.Where(r => r.NodeType == (int)RMNodeLevel.GoogleFile).ToList();
                    var option = SerializerHelper.DeserializeByDataContractSerializer<ChangeTermOption>(actionExtension.ToString());
                    if(files != null && files.Any())
                    {
                        logger.Info("Processing reclassify job action for Google files .");
                        var idList = files.Select(r => r.Id).ToList();
                        option.GoogleDriveRecordIds = idList;
                        int failedCount = await ChangeLabelGoogleForGlobalSearchAsync(files.Select(r => r.Id).ToList(), jobId, option, isJob);
                        logger.Info("Failed count:" + failedCount);
                        mFailedCount += failedCount;
                        mSuccessCount += (files.Count - failedCount);

                    }
                    if (folders != null && folders.Any())
                    {
                        logger.Info("Processing reclassify job action for Google folders .");
                        await ProcessGoogleFolderAsync(ConverLabelOptiontoDto(option), records.Select(r => r.Id).ToList(), jobId);
                    }
                }
                else
                {
                    ChangeTermOption changeTermDto = SerializerHelper.DeserializeByDataContractSerializer<ChangeTermOption>(actionExtension.ToString());

                    var fsFolders = records.Where(r => r.NodeType == (int)NodeLevel.FSFolder).ToList();
                    var nonFolder = records.Where(r => r.NodeType != (int)NodeLevel.FSFolder && r.NodeType != (int)NodeLevel.Folder).ToList();
                    if (nonFolder != null && nonFolder.Count > 0)
                    {
                        PreProcess(flag, changeTermDto, nonFolder.Select(r => r.Id).ToList());
                        int failedCount = await ExplorerService.ChangeTermForGlobalSearchAsync(nonFolder.Select(r => r.Id).ToList(), flag, jobId, changeTermDto, isJob);
                        logger.Info("Failed count:" + failedCount);
                        mFailedCount += failedCount;
                        mSuccessCount += (nonFolder.Count - failedCount);
                    }

                    if (fsFolders != null && fsFolders.Count > 0)
                    {
                        var connectionIdList = fsFolders.Select(a => a.ScopeId).Distinct().ToList();
                        //TODO FS Folder
                        await ProcessFSFolderAsync(changeTermDto, fsFolders.Select(r => r.Id).ToList());
                    }

                    var office365FolderIds = records.Where(r => r.NodeType == (int)NodeLevel.Folder).Select(o => o.Id).ToList();
                    if (office365FolderIds != null && office365FolderIds.Count > 0)
                    {
                        await ProcessOffice365FoldersAsync(flag, changeTermDto, office365FolderIds);
                    }
                }
            }
            catch (Exception e)
            {
                mFailedCount++;
                logger.Error($"An error occurred while doing ReclassifyAction. Error:{e.ToString()}");
            }
            logger.Info("Process reclassify action finished.");
        }
        private ChangeTermDto ConverLabelOptiontoDto(ChangeTermOption option)
        {
            return new ChangeTermDto()
            {
                GoogleDriveRecordIds = option.GoogleDriveRecordIds,
                Comment = option.Comment,
                TermInfo = new TargetTermInfo()
                {
                    UniqueId = option.TargetTermUniqueId,
                    Name = option.TargetTermName
                },
                OverWriteSubFiles = option.OverWriteSubFiles,
                ReclassifySubFiles = option.ReclassifySubFiles,
                UserId = option.LogonUser,
            };
        }
        private void PreProcess(SourceFlag flag, ChangeTermOption changeTermDto, List<Guid> ids)
        {
            switch (flag)
            {
                case SourceFlag.SharePoint:
                    changeTermDto.SourceRecordIds = ids;
                    break;
                case SourceFlag.Exchange:
                    changeTermDto.SourceEXORecordIds = ids;
                    break;
                case SourceFlag.FileSystem:
                    changeTermDto.SourceFSRecordIds = ids;
                    break;
                case SourceFlag.Physical:
                    changeTermDto.SourcePhyRecordIds = ids;
                    break;
                case SourceFlag.OneDrive:
                    changeTermDto.SourceOneDriveRecordIds = ids;
                    break;
                case SourceFlag.AzureFileShare:
                    changeTermDto.SourceAzureFileShareRecordIds = ids;
                    break;
                case SourceFlag.Box:
                    changeTermDto.SourceBoxRecordIds = ids;
                    break;
                case SourceFlag.Teams:
                    changeTermDto.SourceTeamsRecordIds = ids;
                    break;
                case var f when (int)f >= 1000:
                    changeTermDto.SourceCustomizeConnectorRecordIds = ids;
                    break;
            }
        }

        private async Task ProcessFSFolderAsync(ChangeTermOption changeTermDto, List<Guid> ids)
        {
            var records = ExplorerDao.GetRecordByIds(ids);
            FSFolderReclassifier folderReclassifier = new FSFolderReclassifier(ConvertChangeTermOptionToDto(changeTermDto));
            await folderReclassifier.RunForGlobalSearchActionAsync(records, mProcessedFSFolderId);
            mSuccessCount += folderReclassifier.mSucceedCount;
            mFailedCount += folderReclassifier.mFailedCount;
        }

        /// <summary>
        /// Support SP/OneDrive/Teams content source folder.
        /// </summary>
        private async Task ProcessOffice365FoldersAsync(SourceFlag flag, ChangeTermOption changeTermDto, List<Guid> ids)
        {
            var records = ExplorerDao.GetRecordByIds(ids);
            RMReclassifierBase  reclassifier = RMReclassifierFactory.GetInstance(flag, ConvertChangeTermOptionToDto(changeTermDto));
            await reclassifier.RunForGlobalSearchActionAsync(records);
            mSuccessCount += reclassifier.mSucceedCount;
            mFailedCount += reclassifier.mFailedCount;
        }

        private ChangeTermDto ConvertChangeTermOptionToDto(ChangeTermOption option)
        {
            ChangeTermDto dto = new ChangeTermDto()
            {
                OverWriteSubFiles = option.OverWriteSubFiles,
                ReclassifySubFiles = option.ReclassifySubFiles,
                TermInfo = new TargetTermInfo()
                {
                    Id = option.TargetTermId,
                    Name = option.TargetTermName,
                    UniqueId = option.TargetTermUniqueId
                },
                Comment = option.Comment,
                IsManualData = option.IsManualData,
                ChangeTermOrigin = option.ChangeTermOrigin
            };
            return dto;
        }

        public int GetSuccessCount()
        {
            return mSuccessCount;
        }

        public int GetFailedCount()
        {
            return mFailedCount;
        }

        private async Task ProcessGoogleFolderAsync(ChangeTermDto changeTermDto, List<Guid> folderIds, string jobId)
        {
            logger.Info("Start to process Google folders for reclassification.");
            var googleFolderReclassifier = new GoogleFolderReclassifier(jobId, changeTermDto);
            try
            {
                List<Record> folderRecords = ExplorerDao.GetActiveRecordsByIds(folderIds);
                var allFolderFiles = new Dictionary<Guid, List<Record>>();

                foreach (var folder in folderRecords)
                {
                    var filesInFolder = await ExplorerDao.GetAllGoogleFilesByBatchBFSAsync(folder.ScopeId, folder.Id);

                    allFolderFiles[folder.Id] = filesInFolder;
                }
          
                await googleFolderReclassifier.RunForGlobalSearchActionAsync(folderRecords, allFolderFiles);

                int totalRecordCount = allFolderFiles.Values.Sum(list => list?.Count ?? 0);
                mFailedCount += googleFolderReclassifier.FailedCount;

                if (totalRecordCount > 0) 
                {
                    mSuccessCount += totalRecordCount - googleFolderReclassifier.FailedCount;
                }

                logger.Info($"Google folder reclassification with total records is {totalRecordCount}, number records fail {mFailedCount} and records sucess {mSuccessCount}.");
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to reclassify Google folders. Error: {ex.ToString()}");
            }
        }

        public async Task<int> ChangeLabelGoogleForGlobalSearchAsync(List<Guid> recordsId, string jobId, ChangeTermOption changeTermOption, bool isJob)
        {
            int failedCount = 0;
            var googleReclassifyUtil = new RMGoogleDriveExplorerUtility(jobId);
            try
            {
                await googleReclassifyUtil.ChangeLabelGoogleForGlobalSearchAsync(changeTermOption, jobId);
                failedCount = googleReclassifyUtil.FailedCount;
            }
            catch (Exception e)
            {
                logger.Warn("Update labels error {0}", e.ToString());
                failedCount = changeTermOption.SourceRecordIds.Count;
            }        
            return failedCount;
        }
    }
}
