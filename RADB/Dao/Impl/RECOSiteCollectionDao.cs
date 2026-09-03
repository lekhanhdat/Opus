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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Common.Cache;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core;
using AvePoint.RA.CommonUtil;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RECOSiteCollectionDao : BaseDao<RECOSiteCollection>, IRECOSiteCollectionDao
    {
      
        public async Task SaveOrUpdateSiteCollectionAsync(RECOSiteCollection entity)
        {
            //Core.AzureTableStorageUtility.AddAzureTableEntity(CommonRoleConfiguration.AzureTableConnection, _RECOSitePrefix + TenantLocalValue.LogonGroupId.Replace("-", string.Empty), entity);

            using (var ctx = GetNewContext())
            {
                var oldData = ctx.RECOSiteCollection.Where(s => s.SiteId == entity.SiteId && s.CurrentNodeId == entity.CurrentNodeId).FirstOrDefault();
                if (oldData == null)
                {
                    ctx.RECOSiteCollection.Add(entity);
                    ctx.SaveChanges();
                }
                else
                {
                    oldData.CollectDataTime = entity.CollectDataTime;
                    oldData.IsInActive = entity.IsInActive;
                    oldData.IsPhysicalLibrary = entity.IsPhysicalLibrary;
                    oldData.SiteTitle = entity.SiteTitle;
                    await UpdateAsync(oldData);
                }
            }
        }

        public bool CheckIsNull(RECOSiteCollection entit)
        {
            return entit == null;
        }

        public Dictionary<Guid, string> GetAllPhysicalLists()
        {
            // builder.AppendAndQuery("IsPhysicalLibrary", AzureQueryComparisons.Equal, 1, AzureDataType.Int);

            Dictionary<Guid, string> dic = new Dictionary<Guid, string>();
            using var ctx = GetNewContext();
            var datas = ctx.RECOSiteCollection.Where(s => s.IsPhysicalLibrary == 1);
            foreach (var data in datas)
            {
                if (!dic.ContainsKey(data.CurrentNodeId))
                {
                    dic.Add(data.CurrentNodeId, data.SiteId.ToString());
                }
            }
            return dic;
        }

        public long GetLastCollectDateTime(Guid siteCollectionId)
        {
            try
            {
                using var ctx = GetNewContext();
                var data = ctx.RECOSiteCollection.Where(s => s.CurrentNodeId == siteCollectionId && s.SiteId == siteCollectionId).First();
                return data.CollectDataTime;
            }
            catch (Exception e)
            {
                return DateTime.MinValue.Ticks;
            }
        }

        public RECOSiteCollection GetSiteCollectionById(Guid siteCollectionId)
        {
            try
            {

                using var ctx = GetNewContext();
                var data = ctx.RECOSiteCollection.Where(s => s.CurrentNodeId == siteCollectionId && s.SiteId == siteCollectionId).FirstOrDefault();
                return data;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public List<string> GetSiteCollectionWithOutPhysical()
        {
            //builder.AppendAndQuery("IsPhysicalLibrary", AzureQueryComparisons.Equal, 0, AzureDataType.Int);
            using var context = GetNewContext();
            return context.RECOSiteCollection.Where(s => s.IsPhysicalLibrary == 0).Select(g => g.CurrentNodeId.ToString()).ToList();
        }

        public void SetInActiveStatus(List<Guid> siteCollectionIds)
        {
            //AzureTableQueryConditionBuilder builder = new AzureTableQueryConditionBuilder();
            //foreach (var siteCollectionId in siteCollectionIds)
            //{
            //    builder.AppendAndQuery("PartitionKey", AzureQueryComparisons.NotEqual, siteCollectionId.ToString(), AzureDataType.String);
            //}
            //return builder.ToString();
            using var context = GetNewContext();
            var entities = context.RECOSiteCollection.Where(s => !siteCollectionIds.Contains(s.SiteId)).ToList();
            foreach (var itemEntity in entities)
            {
                itemEntity.IsInActive = 1;
            }
            BatchUpdate(entities);
        }

        public Task UpdateLastCollectDateTimeAsync(Guid siteCollectionId, long ticks)
        {
            var context = GetNewContext();
            var data = context.RECOSiteCollection.Where(s => s.SiteId == siteCollectionId && s.CurrentNodeId == siteCollectionId)?.First();
            data.CollectDataTime = ticks;
            return UpdateAsync(data);
        }
    }
}
