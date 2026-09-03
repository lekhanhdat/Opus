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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common.Cache;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RECOTableDao : IRECOTableDao
    {
        private const string _RECODBPrefix = "RECODB";
        public void AddEntity(RECOTableEntity entity)
        {
            Core.AzureTableStorageUtility.AddAzureTableEntity(CommonRoleConfiguration.AzureTableConnection, _RECODBPrefix + TenantLocalValue.LogonGroupId.Replace("-", string.Empty), entity);
        }

        public bool CheckIsNull(RECOTableEntity entit)
        {
            return entit == null;
        }

        public void UpdateLastCollectDateTime(Guid siteCollectionId, long ticks)
        {
            RECOTableEntity entity = Core.AzureTableStorageUtility.RetrieveTableEntity<RECOTableEntity>(CommonRoleConfiguration.AzureTableConnection, _RECODBPrefix + TenantLocalValue.LogonGroupId.Replace("-", string.Empty), siteCollectionId.ToString(), siteCollectionId.ToString());
            entity.CollectionTime = ticks;
            Core.AzureTableStorageUtility.UpdateTableEnity<RECOTableEntity>(CommonRoleConfiguration.AzureTableConnection, _RECODBPrefix + TenantLocalValue.LogonGroupId.Replace("-", string.Empty), entity);
        }

        public void SetInActiveStatus(Guid sitecollectionId, Guid webId, Guid listId, Guid folderId, int itemid)
        {
            string removeQuery = RecordQueryFactory.CreateRemoveObjectQuery(sitecollectionId, webId, listId, folderId, itemid);
            var Entities = Core.AzureTableStorageUtility.RetrieveTableEntitiesInCondition<RECOTableEntity>(CommonRoleConfiguration.AzureTableConnection, _RECODBPrefix + TenantLocalValue.LogonGroupId.Replace("-", string.Empty), removeQuery);
            var updateEntites = new List<RECOTableEntity>();
            if (itemid > 0 && Entities.Count() > 0 && Entities.First().NodeType == (int)NodeLevel.Folder)
            {
                string folderQuery = RecordQueryFactory.CreateRemoveFolderObjectQuery(sitecollectionId, Entities.First().RowKey, listId);
                var itemEntities = Core.AzureTableStorageUtility.RetrieveTableEntitiesInCondition<RECOTableEntity>(CommonRoleConfiguration.AzureTableConnection, _RECODBPrefix + TenantLocalValue.LogonGroupId.Replace("-", string.Empty), folderQuery);
                foreach (var itemEntity in itemEntities)
                {
                    itemEntity.IsInActive = 1;
                    updateEntites.Add(itemEntity);
                }
                Core.AzureTableStorageUtility.UpdateTableEnities<RECOTableEntity>(CommonRoleConfiguration.AzureTableConnection, _RECODBPrefix + TenantLocalValue.LogonGroupId.Replace("-", string.Empty), updateEntites);
                return;
            }
           
            foreach (var entity in Entities)
            {
                entity.IsInActive = 1;//Replace Enum later
                updateEntites.Add(entity);
            }
            Core.AzureTableStorageUtility.UpdateTableEnities<RECOTableEntity>(CommonRoleConfiguration.AzureTableConnection, _RECODBPrefix + TenantLocalValue.LogonGroupId.Replace("-", string.Empty), updateEntites);
            //Core.AzureTableStorageUtility.UpdateTableEnities<RECOTableEntity>(CommonRoleConfiguration.AzureTableConnection, _RECODBPrefix + TenantLocalValue.LogonGroupId.Replace("-", string.Empty), removeQuery);
        }

        public IEnumerable<RECOTableEntity> GetAllBCSData()
        {
            return Core.AzureTableStorageUtility.RetrieveTableEntitiesInCondition<RECOTableEntity>(CommonRoleConfiguration.AzureTableConnection, _RECODBPrefix + TenantLocalValue.LogonGroupId.Replace("-", string.Empty), RecordQueryFactory.CreateGetBCSDataObjectQuery());
        }

        public RECOTableEntity GetEntityBySiteID(Guid siteCollID)
        {
            var entities = Core.AzureTableStorageUtility.RetrieveTableEntitiesInCondition<RECOTableEntity>(CommonRoleConfiguration.AzureTableConnection, _RECODBPrefix + TenantLocalValue.LogonGroupId.Replace("-", string.Empty), RecordQueryFactory.CreateGetSiteByIDQuery(siteCollID));
            return entities.First();
        }
        /// <summary>
        /// not use now....
        /// </summary>
        /// <param name="siteCollectionIds"></param>
        public void SetInActiveStatus(List<Guid> siteCollectionIds)
        {
            var query = RecordQueryFactory.CreateInActiveSiteCollectionQuery(siteCollectionIds);
            var Entities = Core.AzureTableStorageUtility.RetrieveTableEntitiesInCondition<RECOTableEntity>(CommonRoleConfiguration.AzureTableConnection, _RECODBPrefix + TenantLocalValue.LogonGroupId.Replace("-", string.Empty), query);
            foreach (var itemEntity in Entities)
            {
                itemEntity.IsInActive = 1;
            }
            Core.AzureTableStorageUtility.UpdateTableEnities<RECOTableEntity>(CommonRoleConfiguration.AzureTableConnection, _RECODBPrefix + TenantLocalValue.LogonGroupId.Replace("-", string.Empty), Entities);
        }

        public bool CheckAzureTableExist()
        {
            return Core.AzureTableStorageUtility.CheckAzureTableExist(CommonRoleConfiguration.AzureTableConnection, _RECODBPrefix + TenantLocalValue.LogonGroupId.Replace("-", string.Empty));
        }
    }
}
