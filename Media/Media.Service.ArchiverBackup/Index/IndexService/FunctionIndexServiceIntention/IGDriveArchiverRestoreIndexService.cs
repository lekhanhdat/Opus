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
    using AvePoint.Media.Service.DomainModel;

    #endregion using directives

    public interface IGDriveArchiverRestoreIndexService
    {
        List<GoogleBasicIndex> LoadFolders(ArchiverIndexInfo indexInfo);
        List<GoogleBasicIndex> LoadItems(ArchiverIndexInfo indexInfo);
        List<GoogleBasicIndex> LoadItemVersionsByItemId(int topCount, string ItemId, long endTime);
        GoogleBasicIndex Load(String path, long endTime);
        void UpdateRetentionStatus(String path, long endTime);
        GoogleBasicIndex LoadByPathMd5(string pathMd5, long endTime);
        GoogleBasicIndex LoadNeedHoldItem(String jobid, String name, String pathMD5);
        Int64 GetItemsCount(String parentPath, long endTime);
        Dictionary<string, string> GetAllJobEncryptionInfos();
        GoogleBasicIndex GetCurrentIndex(String pathMD5, String subJobId);
        List<GoogleBasicIndex> LoadAttachments(String parentPathMD5, String Name, String type);
        GoogleBasicIndex GetAppWeb(ArchiverIndexInfo indexInfo);
        GoogleBasicIndex LoadNextIndex(GoogleBasicIndex indexInfo);
        GoogleBasicIndex LoadNextBodyIndex(GoogleBasicIndex indexInfo);
        GoogleBasicIndex GetCurrentIndex(String pathMD5);
        GoogleBasicIndex GetParentIndex(String pathMD5);
    }
}