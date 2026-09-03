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
using AvePoint.RA.Common;
using AvePoint.RA.Contract.ArchivedFullTextIndex;
using AvePoint.RA.DB.Model.ArchivedFullTextIndex;
using Cloud.Sdk.Data.EDiscovery;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.ArchivedFullTextIndex.Work.EDiscovery
{
    public class RMArchivedFullTextIndexEDiscoveryDataItemDeletor : RMArchivedFullTextIndexEDiscoveryDataOperator
    {
        public RMArchivedFullTextIndexEDiscoveryDataItemDeletor(
            RMArchivedFullTextIndexSiteManager siteManager, 
            RMArchivedFullTextIndexJobManager jobManager, 
            RMArchivedFullTextIndexSyncJobManager syncJobManager)
            : base(siteManager, jobManager, syncJobManager)
        {
        }

        protected override Cloud.Sdk.Data.EDiscovery.IndexType OperateType => Cloud.Sdk.Data.EDiscovery.IndexType.Delete;

        protected override string OperateName => "ItemDelete";

        private readonly AppendState _appendState = new();

        public async Task<bool> DeleteAsync(string indexDBUniqueId)
        {
            try
            {
                var fieldList = BuildDeleteQueryGroup(indexDBUniqueId);
                var fieldJson = JsonConvert.SerializeObject(fieldList);

                await AppendLineAndUploadIfNeededAsync(
                    fieldJson,
                    _appendState,
                    "Write delete item info to local file",
                    $"The item deletor [{_siteManager.SiteUrl}] [{_syncJobManager.ArchiverJobId}] data is reach limit, need to upload data to e-discovery.");

                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while delete item [{indexDBUniqueId}]. Error: {e}");
                return false;
            }
        }

        private static List<QueryGroup> BuildDeleteQueryGroup(string indexDBUniqueId)
        {
            return new List<QueryGroup>
            {
                new QueryGroup
                {
                    QueryFields = new List<FieldQuery>
                    {
                        new FieldQuery
                        {
                            Field = new Field
                            {
                                Name = "indexDBUniqueId",
                                Value = indexDBUniqueId,
                                FieldType = FieldType.String
                            },
                            Operator = FilterOperator.And
                        }
                    },
                    Operator = FilterOperator.And
                }
            };
        }
    }
}
