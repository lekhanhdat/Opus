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
using AvePoint.RA.Contract.Discovery.Model.Query.Google.Parameter;
using AvePoint.RA.Contract.Discovery.Model.Query.Google.Parameter.Profile;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Google.Query.Profile.Inactive
{
    public class RMDiscoveryGoogleInactiveProfileNodeTotalAggregateDataQuerier : RMDiscoveryGoogleInactiveProfileDataQuerier<Dictionary<string, object>>
    {
        public RMDiscoveryGoogleInactiveProfileNodeTotalAggregateDataQuerier(RMDiscoveryGoogleProfileQueryParameter queryParameter) : base(queryParameter)
        {
        }

        public override async Task<Dictionary<string, object>> QueryAsync()
        {

            var sql = $@"SELECT 
data.FileTotalSize AS fileTotalSize, 
data.FileSumCount AS fileSumCount, 
data.InactiveFileTotalSize AS inactiveFileTotalSize,
data.InactiveFileSumCount AS inactiveFileSumCount
 FROM [{_profileSchemaName}].[RMGoogleProfileBasicInactiveData] as data";

            var list = await _queryDao.GetDataDictionaryListAsync(sql);

            var res = new Dictionary<string, object>
            {
                {"inactiveFileTotalSize", 0L},
                {"inactiveFileSumCount", 0L},
                {"fileTotalSize", 0L },
                {"fileSumCount", 0L },
            };

            if (list.Count > 0)
            {
                var data = list[0];
                res["inactiveFileTotalSize"] = Convert.ToInt64(data["inactiveFileTotalSize"]);
                res["inactiveFileSumCount"] = Convert.ToInt64(data["inactiveFileSumCount"]);
                res["fileTotalSize"] = Convert.ToInt64(data["fileTotalSize"]);
                res["fileSumCount"] = Convert.ToInt64(data["fileSumCount"]);
            }


            if (_queryParameter.NodeQueryParameter.ViewMode == RMDiscoveryGoogleNodeViewMode.Container)
            {
                var driveCount = await _nodeDao.CountDiscoveryGoogleDriveAsync(_queryParameter.OrganizationId);
                res["driveCount"] = driveCount;
            }

            return res;
        }
    }
}
