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



namespace Office365GroupRestore
{
    #region using directives

    using AvePoint.Media.Service.DomainModel;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    #endregion using directives

    public interface IExchangeContainerAndItemIndexService
    {
        List<GroupBasicIndex> GetSubContainers(ExchangeIndexInfo parentIndexInfo);

        List<String> GetTopicIds(ExchangeIndexInfo parentIndexInfo);

        bool IsTopicIdHistoryExist(ExchangeIndexInfo parentIndexInfo);

        List<long> GetItemCreatedTime(ExchangeIndexInfo parentIndexInfo);

        List<GroupBasicIndex> GetSubItems(ExchangeIndexInfo parentIndexInfo);

        List<GroupBasicIndex> LoadConversationItems(ExchangeIndexInfo parentIndexInfo);

        Int32 GetSubItemsCount(ExchangeIndexInfo parentIndexInfo);

        Int32 GetOneConversationItemsCount(ExchangeIndexInfo parentIndexInfo);

        void Insert(List<GroupBasicIndex> indexes);

        GroupBasicIndex GetOneData(bool isContainer, ExchangeIndexInfo indexInfo);

        List<String> GetEntireCycleStorageInfos();

        List<String> GetStorageInfosExceptFullBackup();

        List<String> GetStorageInfosByJobId(String jobId);

        GroupBasicIndex GetParentFolder(GroupBasicIndex childIndex);

        void DeleteItemByJobId(String jobId);

        List<GroupBasicIndex> Search(StringBuilder sql, FilterInfo filter, ExchangeBrowseInfo restoreParam);

        void ProcessColumnUpgrate();

        void CreateIndex(String columnName);
        long? GetOldestMessageCreateDate();
        Dictionary<string, string> LoadEXONameAndMd5Mapping();
        List<ExchangeBasicIndex> GetItemsByParentMd5(string parentMd5);
        List<ArchiverBasicIndex> GetArchiverBasicIndexItemsInHeadByParentPathMd5(string parentPath);
        ArchiverBasicIndex GetArchiverBasicIndexByPathMd5(string pathMd5);
        List<ArchiverBasicIndex> GetArchiverBasicIndexItemsInBodyByParentPathMd5(string parentPathMd5);
    }
}