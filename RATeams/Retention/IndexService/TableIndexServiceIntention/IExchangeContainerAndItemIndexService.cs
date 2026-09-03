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




namespace Office365GroupRetention
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    using AvePoint.Media.Service.DomainModel;
    #endregion using directives

    public interface IExchangeContainerAndItemIndexService
    {
        void UpdateAccessTier(int tier, string jobid);

        void UpdateRetentionStatus(string storagePolicyId, string jobid);

        void UpdateAsSoftDelete(String storagePolicyId, String jobId);

        bool IsExistsIndexRelatedToJob(string jobId);

        List<GroupBasicIndex> GetSubContainers(ExchangeIndexInfo parentIndexInfo);

        List<GroupBasicIndex> GetSubItems(ExchangeIndexInfo parentIndexInfo);

        Int32 GetSubItemsCount(ExchangeIndexInfo parentIndexInfo);

        void Insert(List<GroupBasicIndex> indexes);

        GroupBasicIndex GetOneData(ExchangeIndexInfo indexInfo);

        List<String> GetEntireCycleStorageInfos();

        List<String> GetStorageInfosExceptFullBackup();

        List<String> GetStorageInfosByJobId(String jobId);

        GroupBasicIndex GetParentFolder(GroupBasicIndex childIndex);

        void DeleteItemByJobId(String jobId);

        List<ArchiverBasicIndex> GetDeletingDataFromMainIndex(String storagePolicyId, String jobId);

        void DeleteContainerAndItemIndexByStorageAndJobId(String storagePolicyId, String jobId);

        List<GroupBasicIndex> Search(StringBuilder sql, FilterInfo filter, ExchangeBrowseInfo restoreParam);

        void UpdateFormerJobIdToCurrentJobId(String jobId);

        void DeleteDeleteTypeData();

        //void UpdateDuplicatedData();

        void UpdateHasAttachColumn();

        void CreateIndexContainerAndItemIndex();

        Int64 GetRepeatContainerCount(String jobId);

        Int64 GetIndexTotalCount(String jobId);

        List<GroupBasicIndex> GetNeedFiles(String jobId, Int32 offset, Int32 length);

        List<GroupBasicIndex> GetMetaDataIndexs();

        List<GroupBasicIndex> GetContentDataIndexs();

        Int64 GetContainerIndexTotalCount(String jobId);

        Int64 GetItemIndexTotalCount(String jobId);

        Int64 GetItemTotalSize(String jobId);

        void DeleteContainerAndItemIndexByJobId(String jobId);

        void ProcessColumnUpgrate();

        int GetContainerCount();

        bool HasContainter(string pathMd5);
    }
}