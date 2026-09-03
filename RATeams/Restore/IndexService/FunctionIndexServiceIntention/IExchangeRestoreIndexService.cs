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

    public interface IExchangeRestoreIndexService
    {
        GroupBasicIndex Load(bool isContainer, ExchangeIndexInfo indexInfo);

        List<GroupBasicIndex> LoadFolders(ExchangeIndexInfo indexInfo);

        List<GroupBasicIndex> LoadItems(ExchangeIndexInfo indexInfo);

        List<GroupBasicIndex> LoadConversationItems(ExchangeIndexInfo indexInfo);
        List<String> GetTopicIds(ExchangeIndexInfo indexInfo);
        bool IsTopicIdHistoryExist(ExchangeIndexInfo indexInfo);
        List<long> GetItemCreatedTime(ExchangeIndexInfo indexInfo);

        long GetItemsCount(ExchangeIndexInfo indexInfo);
        Dictionary<string, string> LoadEXONameAndMd5Mapping();
        List<ExchangeBasicIndex> GetItemsByParentMd5(string parentMd5);

        long GetOneConversationItemsCount(ExchangeIndexInfo indexInfo);

        void ProcessColumnUpgrate();

        void CreateIndex(String columnName);
        List<ArchiverBasicIndex> GetArchiverBasicIndexItemsByParentPathMd5(string parentPath);
        ArchiverBasicIndex GetArchiverBasicIndexByPathMd5(string pathMd5);
        List<ArchiverBasicIndex> GetArchiverBasicIndexItemsInBodyByParentPathMd5(string parentPathMd5);
        long? GetOldestMessageCreateDate();
    }
}