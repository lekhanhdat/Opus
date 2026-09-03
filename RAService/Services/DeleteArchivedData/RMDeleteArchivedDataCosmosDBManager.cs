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
using AvePoint.Media.Service.DomainModel;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.DB.AzureCosmosDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.DeleteArchivedData
{
    public class RMDeleteArchivedDataCosmosDBManager
    {
        private static readonly RALogger _logger = RALogger.GetInstance(typeof(RMDeleteArchivedDataCosmosDBManager));

        private static bool _existsContainer;

        private static RMAzureCosmosDBContainer _container;

        public static async Task InitAsync()
        {
            _existsContainer = await RMAzureCosmosDBContext.ExistsContainer();
            _logger.Info($"The current customer has cosmos db container? [{_existsContainer}]");
            if (_existsContainer)
            {
                _container = await RMAzureCosmosDBContext.GetContainerAsync(false);
            }
        }

        public static async Task<bool> DeleteItemAsync(Guid siteUniqueId, ArchiverBasicIndex item)
        {
            try
            {
                if(!_existsContainer)
                {
                    return true;
                }

                if(item.Name.Contains(":"))
                {
                    _logger.Warn($"The site [{item.SitePath}] item [{item.PathMD5}] is version level, no need delete in cosmos db.");
                    return true;
                }

                var existsItem = await _container.UseLinqQuery().Where(cosmosItem => cosmosItem.ScopeId == siteUniqueId &&
                    cosmosItem.NodeId == new Guid(item.NodeGuid) &&
                    cosmosItem.RecordStatus != 10).AsResultSet().FirstOrDefault();
                if(existsItem == null)
                {
                    _logger.Info($"The site [{item.SitePath}] item [{item.PathMD5}] not found in cosmos db.");
                    return true;
                }

                existsItem.RecordStatus = (int)RMRecordStatus.Retention;
                existsItem.ManualArchiveStatus = (int)ActionStatus.Archiverd;
                await _container.UpsertAsync(existsItem);
                _logger.Info($"The site [{item.SitePath}] item [{item.PathMD5}] has been deleted in cosmso db.");
                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while delete site [{item.SitePath}] item [{item.PathMD5}] in cosmos db. Error: {e}");
                return false;
            }
        }
    }
}
