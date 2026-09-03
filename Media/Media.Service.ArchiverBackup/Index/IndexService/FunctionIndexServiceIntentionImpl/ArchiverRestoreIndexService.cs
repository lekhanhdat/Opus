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




namespace AvePoint.Media.Service.ArchiverBackup
{
    #region using directives
    using System.Collections.Generic;
    using System.Text;
    using AvePoint.GCommon.Contract.Server.Common.BackupDataSearch;
    using AvePoint.Media.Common;
    using AvePoint.Media.Service.DomainModel;
    using System;

    #endregion using directives

    public class ArchiverRestoreIndexService
        : ArchiverIndexServiceBase
        , IArchiverRestoreIndexService
    {
        public List<ArchiverBasicIndex> LoadFolders(ArchiverIndexInfo indexInfo, StringBuilder? sql = null, BackupDataSearchContract? searchContract = null)
        {
            if (sql != null)
            {
                return this.HeadAndBodyService.GetDatasFromHeadTableByFilter(indexInfo, sql, searchContract);
            }

            return this.HeadAndBodyService.GetDatasFromHeadTable(indexInfo);
        }

        public List<ArchiverBasicIndex> LoadItems(ArchiverIndexInfo indexInfo, StringBuilder? sql = null, BackupDataSearchContract? searchContract = null)
        {
            if (sql != null && searchContract != null)
            {
                return this.HeadAndBodyService.GetDatasFromBodyTableByFilter(indexInfo, sql, searchContract);
            }
            return this.HeadAndBodyService.GetDatasFromBodyTable(indexInfo);
        }

        public List<ArchiverBasicIndex> LoadCurrentItems(ArchiverIndexInfo indexInfo)
        {
            return this.HeadAndBodyService.GetCurrentItemsFromBodyTable(indexInfo);
        }

        public List<ArchiverBasicIndex> LoadItemVersionsByItemId(int topCount, string ItemId, long endTime, bool isRestoreAllVersions)
        {
            return this.HeadAndBodyService.GetVersionsByItemIdFromBodyTable(topCount, ItemId, endTime, isRestoreAllVersions);
        }

        public Dictionary<string, List<ArchiverBasicIndex>> LoadItemVersionsByItemIds(int topCount, List<string> itemIds, long endTime, bool isRestoreAllVersions)
        {
            return this.HeadAndBodyService.GetVersionsByItemIdsFromBodyTable(topCount, itemIds, endTime, isRestoreAllVersions);
        }

        public ArchiverBasicIndex GetAppWeb(ArchiverIndexInfo indexInfo)
        {
            return this.HeadAndBodyService.GetAppWeb(indexInfo);
        }

        public ArchiverBasicIndex Load(string path, long endTime)
        {
            return this.HeadAndBodyService.GetOneDataFromHeadOrBodyTable(path, endTime);
        }
        public void UpdateRetentionStatus(String path, long endTime, BackupDataSearchContract? searchContract = null)
        {
            if (searchContract != null)
            {
                // path here is the parent path, not the item path
                this.HeadAndBodyService.UpdateRetentionStatusByFilter(path, endTime, searchContract);
                return;
            }
            this.HeadAndBodyService.UpdateRetentionStatus(path, endTime);
        }
        public ArchiverBasicIndex LoadByPathMd5(string pathMd5, long endTime)
        {
            return this.HeadAndBodyService.GetOneDataFromHeadByPathMd5(pathMd5, endTime);
        }

        public long GetItemsCount(string parentPath, long endTime, StringBuilder? sql = null, BackupDataSearchContract? searchContract = null)
        {
            if (sql != null && searchContract != null)
            {
                return this.HeadAndBodyService.GetDatasCountFromBodyTableByFilter(parentPath, endTime, sql, searchContract);
            }
            return this.HeadAndBodyService.GetDatasCountFromBodyTable(parentPath, endTime);
        }

        public Dictionary<string, string> GetAllJobEncryptionInfos()
        {
            Dictionary<string, string> encryptionInfos = new Dictionary<string, string>();
            var indexList = this.JobInfoIndexService.GetJobInfoIndexesByKey(ServiceConstants.EncryptionInfoKey);
            foreach (ArchiverJobInfoIndex jobInfoIndex in indexList)
            {
                encryptionInfos.Add(jobInfoIndex.JobId, jobInfoIndex.Value);
            }
            return encryptionInfos;
        }

        public ArchiverBasicIndex LoadNeedHoldItem(string jobid, string name, string pathMD5)
        {
            return this.HeadAndBodyService.GetNeedHoldItemFromHeadTable(jobid, name, pathMD5);
        }

        public ArchiverBasicIndex GetCurrentIndex(String pathMD5, String subJobId)
        {
            return this.HeadAndBodyService.GetIndex(pathMD5, subJobId);
        }

        public List<ArchiverBasicIndex> LoadAttachments(String parentPathMD5, String Name, String type)
        {
            return this.HeadAndBodyService.GetAttachments(parentPathMD5, Name, type);
        }

        public ArchiverBasicIndex LoadNextIndex(ArchiverBasicIndex indexInfo)
        {
            return this.HeadAndBodyService.GetNextIndexBySequence(indexInfo.JobId, indexInfo.Sequence);
        }
        public ArchiverBasicIndex LoadNextBodyIndex(ArchiverBasicIndex indexInfo)
        {
            return this.HeadAndBodyService.GetNextBodyIndexBySequence(indexInfo.JobId, indexInfo.Sequence);
        }

        public ArchiverBasicIndex GetCurrentIndex(string pathMD5)
        {
            return this.HeadAndBodyService.GetIndex(pathMD5);
        }

        public ArchiverBasicIndex GetParentIndex(string pathMD5)
        {
            return this.HeadAndBodyService.GetParentIndex(pathMD5);
        }
    }
}