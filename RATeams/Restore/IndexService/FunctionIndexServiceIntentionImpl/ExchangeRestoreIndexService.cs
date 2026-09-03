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

    public class ExchangeRestoreIndexService : ExchangeFunctionIndexServiceBase, IExchangeRestoreIndexService
    {
        public GroupBasicIndex Load(bool isContainer, ExchangeIndexInfo indexInfo)
        {
            return this.ContainerItemIndexService.GetOneData(isContainer,indexInfo);
        }
        public Dictionary<string, string> LoadEXONameAndMd5Mapping()
        {
            return this.ContainerItemIndexService.LoadEXONameAndMd5Mapping();
        }
        public List<GroupBasicIndex> LoadFolders(ExchangeIndexInfo indexInfo)
        {
            return this.ContainerItemIndexService.GetSubContainers(indexInfo);
        }

        public List<GroupBasicIndex> LoadItems(ExchangeIndexInfo indexInfo)
        {
            return this.ContainerItemIndexService.GetSubItems(indexInfo);
        }

        public List<GroupBasicIndex> LoadConversationItems(ExchangeIndexInfo indexInfo)
        {
            return this.ContainerItemIndexService.LoadConversationItems(indexInfo);
        }

        public List<string> GetTopicIds(ExchangeIndexInfo indexInfo)
        {
            return this.ContainerItemIndexService.GetTopicIds(indexInfo);
        }

        public bool IsTopicIdHistoryExist(ExchangeIndexInfo indexInfo)
        {
            return this.ContainerItemIndexService.IsTopicIdHistoryExist(indexInfo);
        }

        public List<long> GetItemCreatedTime(ExchangeIndexInfo indexInfo)
        {
            return this.ContainerItemIndexService.GetItemCreatedTime(indexInfo);
        }

        public long GetItemsCount(ExchangeIndexInfo indexInfo)
        {
            return this.ContainerItemIndexService.GetSubItemsCount(indexInfo);
        }

        public long GetOneConversationItemsCount(ExchangeIndexInfo indexInfo)
        {
            return this.ContainerItemIndexService.GetOneConversationItemsCount(indexInfo);
        }

        public void ProcessColumnUpgrate()
        {
            this.ContainerItemIndexService.ProcessColumnUpgrate();
        }

        public void CreateIndex(String columnName)
        {
            this.ContainerItemIndexService.CreateIndex(columnName);
        }

        public List<ExchangeBasicIndex> GetItemsByParentMd5(string parentMd5)
        {
            return this.ContainerItemIndexService.GetItemsByParentMd5(parentMd5);
        }

        public List<ArchiverBasicIndex> GetArchiverBasicIndexItemsByParentPathMd5(string parentPath)
        {
            return this.ContainerItemIndexService.GetArchiverBasicIndexItemsInHeadByParentPathMd5(parentPath);
        }

        public ArchiverBasicIndex GetArchiverBasicIndexByPathMd5(string pathMd5)
        {
            return this.ContainerItemIndexService.GetArchiverBasicIndexByPathMd5(pathMd5);
        }

        public List<ArchiverBasicIndex> GetArchiverBasicIndexItemsInBodyByParentPathMd5(string parentPathMd5)
        {
            return this.ContainerItemIndexService.GetArchiverBasicIndexItemsInBodyByParentPathMd5(parentPathMd5);
        }

        public long? GetOldestMessageCreateDate()
        {
            return this.ContainerItemIndexService.GetOldestMessageCreateDate();
        }
    }
}