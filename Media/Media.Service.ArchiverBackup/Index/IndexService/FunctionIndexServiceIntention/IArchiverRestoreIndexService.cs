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
    using System;
    using System.Collections.Generic;
    using System.Text;
    using AvePoint.GCommon.Contract.Server.Common.BackupDataSearch;
    using AvePoint.Media.Service.DomainModel;

    #endregion using directives

    public interface IArchiverRestoreIndexService
    {
        List<ArchiverBasicIndex> LoadFolders(ArchiverIndexInfo indexInfo, StringBuilder? sql = null, BackupDataSearchContract? searchContract = null);
        List<ArchiverBasicIndex> LoadItems(ArchiverIndexInfo indexInfo, StringBuilder? sql = null, BackupDataSearchContract? searchContract = null);
        List<ArchiverBasicIndex> LoadCurrentItems(ArchiverIndexInfo indexInfo);
        List<ArchiverBasicIndex> LoadItemVersionsByItemId(int topCount, string ItemId, long endTime,bool isRestoreAllVersions);
        Dictionary<string, List<ArchiverBasicIndex>> LoadItemVersionsByItemIds(int topCount, List<string> itemIds, long endTime, bool isRestoreAllVersions);
        ArchiverBasicIndex Load(String path, long endTime);
        void UpdateRetentionStatus(String path, long endTime, BackupDataSearchContract? searchContract = null);
        ArchiverBasicIndex LoadByPathMd5(string pathMd5, long endTime);
        ArchiverBasicIndex LoadNeedHoldItem(String jobid, String name, String pathMD5);
        Int64 GetItemsCount(String parentPath, long endTime, StringBuilder? sql = null, BackupDataSearchContract? searchContract = null);
        Dictionary<string, string> GetAllJobEncryptionInfos();
        ArchiverBasicIndex GetCurrentIndex(String pathMD5, String subJobId);
        List<ArchiverBasicIndex> LoadAttachments(String parentPathMD5, String Name, String type);
        ArchiverBasicIndex GetAppWeb(ArchiverIndexInfo indexInfo);
        ArchiverBasicIndex LoadNextIndex(ArchiverBasicIndex indexInfo);
        ArchiverBasicIndex LoadNextBodyIndex(ArchiverBasicIndex indexInfo);
        ArchiverBasicIndex GetCurrentIndex(String pathMD5);
        ArchiverBasicIndex GetParentIndex(String pathMD5);
    }
}