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
    using AvePoint.Media.Common;
    using AvePoint.Media.Service.DomainModel;
    using System;
    using global::Media.Service.ArchiverBackup.Index.IndexService.FunctionIndexServiceIntentionImpl;
    using DocumentFormat.OpenXml.VariantTypes;

    #endregion using directives

    public class GDriveArchiverRestoreIndexService
        : GDriveArchiverIndexServiceBase
        , IGDriveArchiverRestoreIndexService
    {
        public List<GoogleBasicIndex> LoadFolders(ArchiverIndexInfo indexInfo)
        {
            return this.HeadAndBodyService.GetDatasFromHeadTable(indexInfo);
        }

        public List<GoogleBasicIndex> LoadItems(ArchiverIndexInfo indexInfo)
        {
            return this.HeadAndBodyService.GetDatasFromBodyTable(indexInfo);
        }
        public List<GoogleBasicIndex> LoadItemVersionsByItemId(int topCount, string ItemId, long endTime)
        {
            return this.HeadAndBodyService.GetVersionsByItemIdFromBodyTable(topCount, ItemId, endTime);
        }
        public GoogleBasicIndex GetAppWeb(ArchiverIndexInfo indexInfo)
        {
            return null;//his.HeadAndBodyService.GetAppWeb(indexInfo);
        }

        public GoogleBasicIndex Load(string path, long endTime)
        {
            return this.HeadAndBodyService.GetOneDataFromHeadOrBodyTable(path, endTime);
        }
        public void UpdateRetentionStatus(String path, long endTime)
        {
            //this.HeadAndBodyService.UpdateRetentionStatus(path, endTime);
        }
        public GoogleBasicIndex LoadByPathMd5(string pathMd5, long endTime)
        {
            return null;//this.HeadAndBodyService.GetOneDataFromHeadByPathMd5(pathMd5, endTime);
        }
        public long GetItemsCount(string parentPath, long endTime)
        {
            return 1;//this.HeadAndBodyService.GetDatasCountFromBodyTable(parentPath, endTime);
        }

        public Dictionary<string, string> GetAllJobEncryptionInfos()
        {
            Dictionary<string, string> encryptionInfos = new Dictionary<string, string>();
            var indexList = new List<ArchiverJobInfoIndex>();//this.SiteMasterService.GetJobInfoIndexesByKey(ServiceConstants.EncryptionInfoKey);
            foreach (ArchiverJobInfoIndex jobInfoIndex in indexList)
            {
                encryptionInfos.Add(jobInfoIndex.JobId, jobInfoIndex.Value);
            }
            return encryptionInfos;
        }

        public GoogleBasicIndex LoadNeedHoldItem(string jobid, string name, string pathMD5)
        {
            return null;// this.HeadAndBodyService.GetNeedHoldItemFromHeadTable(jobid, name, pathMD5);
        }

        public GoogleBasicIndex GetCurrentIndex(String pathMD5, String subJobId)
        {
            return null;//this.HeadAndBodyService.GetIndex(pathMD5, subJobId);
        }

        public List<GoogleBasicIndex> LoadAttachments(String parentPathMD5, String Name, String type)
        {
            return null;//this.HeadAndBodyService.GetAttachments(parentPathMD5, Name, type);
        }

        public GoogleBasicIndex LoadNextIndex(GoogleBasicIndex indexInfo)
        {
            return null;//this.HeadAndBodyService.GetNextIndexBySequence(indexInfo.JobId, indexInfo.Sequence);
        }
        public GoogleBasicIndex LoadNextBodyIndex(GoogleBasicIndex indexInfo)
        {
            return null;//this.HeadAndBodyService.GetNextBodyIndexBySequence(indexInfo.JobId, indexInfo.Sequence);
        }

        public GoogleBasicIndex GetCurrentIndex(string pathMD5)
        {
            return null;//this.HeadAndBodyService.GetIndex(pathMD5);
        }

        public GoogleBasicIndex GetParentIndex(string pathMD5)
        {
            return null;//this.HeadAndBodyService.GetParentIndex(pathMD5);
        }
    }
}