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
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Archiver
{
    public interface IMArchiverJobManagementService
    {
        string InsertIntoArchiverSiteMasterIndex(ArchiverSiteMasterIndexContract indexDto, string groupId);

        /// <summary>
        /// 更新Merge Index State, siteInfo中要包含site collection信息和Farm信息
        /// </summary>
        System.Threading.Tasks.Task UpdateMergeIndexStateAsync(string jobId, ArchiverSiteInfoDto siteInfo, MergeIndexState mergeIndexState, string groupId);
        System.Threading.Tasks.Task UpdateGDriveMergeIndexStateAsync(string jobId, ArchiverSiteInfoDto siteInfo, MergeIndexState mergeIndexState);

        /// <summary>
        /// retention后通知manager去修改sub master表或者delete job record
        /// </summary>
        /// <param name="retentionInfo"></param>
        void NotificationRetentionState(ArchiverPruningJob retentionInfo, string groupId);

        void UpdateMainIndexStorageInfo(string siteCollectionUrl, string storageInfo, string groupId);

        void UpdateSubIndexStorageInfo(string subJobId, string storageInfo, string groupId);

        System.Threading.Tasks.Task UpdateSiteMasterMediaDataSizeAsync(string subjobId, long mediaDataSize, string groupId);

        void UpdateSiteCollectionAfterAchivered(string scUrl, bool isArchivered, string groupId);

        void UpdateSiteCollectionAfterAchivered(string scUrl, bool isArchivered, string groupId, string jobId);

        void AddSiteCollectionAfterRestore(RemoteSiteCollection siteCollection, string sitesGroupName, string groupId);

        void UpdateIndexDevice(string destIndexDeviceId, string groupId);

        bool CheckCurrentJobHasMerged(string jobId, string groupId);

        List<string> GetAllArchiverIndexSubInfo(string jobId, string groupId);

        void UpdateEndUserJobStatisticsByJobId(string jobId, string value, string groupId);

        byte[] GetEndUserStubLinkMasterKey(string groupId);
    }
}
