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
    public class RMArchivedFullTextIndexEDiscoveryDataJobDeletor : RMArchivedFullTextIndexEDiscoveryDataOperator
    {
        public RMArchivedFullTextIndexEDiscoveryDataJobDeletor(
            RMArchivedFullTextIndexSiteManager siteManager, 
            RMArchivedFullTextIndexJobManager jobManager, 
            RMArchivedFullTextIndexSyncJobManager syncJobManager)
            : base(siteManager, jobManager, syncJobManager)
        {
        }

        protected override IndexType OperateType => IndexType.Delete;

        protected override string OperateName => "JobDelete";

        public async Task<bool> DeleteAsync()
        {
            try
            {
                var queryGroup = BuildDeleteQueryGroup();
                var queryGroupJson = JsonConvert.SerializeObject(queryGroup);
                File.Create(_dataFilePath).Dispose();

                using (var fileStream = File.Open(_dataFilePath, FileMode.Append))
                {
                    using var streamWriter = new StreamWriter(fileStream);
                    await streamWriter.WriteLineAsync(queryGroupJson);
                }

                _logger.Info($"The job deletor [{_siteManager.SiteUrl}] [{_syncJobManager.ArchiverJobId}] need to upload data to e-discovery.");

                await UploadDataAsync();

                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while delete job [{_siteManager.SiteUrl}] [{_syncJobManager.ArchiverJobId}] data. Error: {e}");
                return false;
            }
        }

        private List<QueryGroup> BuildDeleteQueryGroup()
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
                                Name = "archiverSubJobId",
                                Value = _syncJobManager.ArchiverJobId,
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
