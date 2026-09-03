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
    using AvePoint.Media.Service.DomainModel;
    using RAFileSystem.FileSystem.Common;
    using RAFileSystem.FileSystem.FileSystem.Backup.CoreIndex.CoreIndexCommon;

    #endregion

    public class ArchiverBackupIndexService
        : ArchiverIndexServiceBase
        , IArchiverBackupIndexService
    {
        private Dictionary<string, ArchiveIndexInfo> masterIndexDic = new Dictionary<string, ArchiveIndexInfo>();

        public void InsertArchiveIndexes(List<ArchiverBasicIndex> indexes)
        {
            this.HeadAndBodyService.InsertArchiveIndexes(indexes);
        }

        public void InsertSiteMaster(ArchiveIndexInfo siteMasterIndex)
        {
            this.SiteMasterService.InsertSiteMaster(siteMasterIndex);
        }
        public ArchiveIndexInfo GetSiteMasterByJobId(string jobId)
        {
            return this.SiteMasterService.GetSiteMasterByJobId(jobId);
        }
        public void UpdateJobInfoIndex(string jobId, string key, string value)
        {
            this.JobInfoIndexService.UpdateJobInfoIndex(jobId, key, value);
        }
        public ArchiverBasicIndex GetCurrentIndex(string pathMD5)
        {
            return this.HeadAndBodyService.GetIndex(pathMD5);
        }
        public ArchiverBasicIndex GetBodyIndexByMD5(string pathMD5)
        {
            var result = this.HeadAndBodyService.GetBodyIndexByMD5(pathMD5);
            var masterIndex = GetMasterIndexInternal(result.JobId);
            DtoConverter.SetMasterIndexValue(masterIndex, result);
            return result;
        }
        private ArchiveIndexInfo GetMasterIndexInternal(string jobId)
        {
            if (masterIndexDic.ContainsKey(jobId))
            {
                return masterIndexDic[jobId];
            }
            else
            {
                var masterIndexTemp = this.SiteMasterService.GetSiteMasterByJobId(jobId);
                masterIndexDic.Add(jobId, masterIndexTemp);
                return masterIndexTemp;
            }
        }
        public ArchiverBasicIndex LoadNextBodyIndex(ArchiverBasicIndex indexInfo)
        {
            var result = this.HeadAndBodyService.GetNextBodyIndexBySequence(indexInfo.JobId, indexInfo.Sequence);
            if (result == null)
            {
                return null;
            }
            var masterIndex = GetMasterIndexInternal(result.JobId);
            DtoConverter.SetMasterIndexValue(masterIndex, result);
            return result;
        }
        public ArchiverBasicIndex LoadNextIndex(ArchiverBasicIndex indexInfo)
        {
            var result = this.HeadAndBodyService.GetNextIndexBySequence(indexInfo.JobId, indexInfo.Sequence);
            if (result == null)
            {
                return null;
            }
            var masterIndex = GetMasterIndexInternal(result.JobId);
            DtoConverter.SetMasterIndexValue(masterIndex, result);
            return result;
        }
        public void InitIndexProcesser(ArchiverIndexService indexService)
        {
            this.HeadAndBodyService.InitIndexProcesser(indexService);
            this.SiteMasterService.InitIndexProcesser(indexService);
        }
    }
}